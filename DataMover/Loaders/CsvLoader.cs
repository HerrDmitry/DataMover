using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMover.Logger;

namespace DataMover.Loaders
{
    public class CsvLoader : ILoader
    {
        private ConcurrentQueue<DataRow> _buffer=new ConcurrentQueue<DataRow>();
        private StreamReader _reader;
        private volatile bool _eof;
        private char _textQualifier;
        private char _delimiter;
        private Task _readerTask;
        private int _bufferSizeRows;
        private int _readBufferSize;
        
        private const int READ_BUFFER_SIZE = 1024 * 1024;
        private const int MAX_LINE_SIZE = 1024 * 1024*20;

        public CsvLoader(char textQualifier = '"', char delimiter = ',', int bufferSizeRows=10000, int readBufferSize=READ_BUFFER_SIZE)
        {
            _textQualifier = textQualifier;
            _delimiter = delimiter;
            _bufferSizeRows = bufferSizeRows;
            _readBufferSize = readBufferSize;
        }

        public IEnumerable<DataRow> ReadLines(Stream source)
        {
            _reader=new StreamReader(source);
            _readerTask = Task.Run(() => FastReadLineTask());
            
            while (!_eof || _buffer.Count>0)
            {
                while (_buffer.TryDequeue(out DataRow line))
                {
                    yield return line;
                }

                if (!_eof)
                {
                    Thread.Sleep(50);
                }
            }
        }
        
        private unsafe void FastReadLineTask()
        {
            try
            {
                var cBuff = new char[_readBufferSize];
                fixed (char* bPtr = cBuff)
                {
                    long counter = 0;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var bufferLength = _reader.ReadBlock(cBuff, 0, _readBufferSize);
                    var bufferPosition = 0;
                    var lastelapsed = 0d;
                    while (!_reader.EndOfStream || bufferPosition < bufferLength)
                    {
                        bool hasLine = false;
                        int qualifierCount = 0;
                        char[] line = null;
                        var startPosition = bufferPosition;
                        var lineLength = 0;

                        void appendToLine()
                        {
                            if (MAX_LINE_SIZE < lineLength + bufferPosition - startPosition)
                            {
                                throw new FileLoadException("Line is too big");
                            }
                            var newLineLength = lineLength + bufferPosition - startPosition;
                            var newLine = new char[newLineLength];
                            if (lineLength > 0)
                            {
                                Array.Copy(line, newLine, lineLength);
                            }
                            Array.Copy(cBuff, startPosition, newLine, lineLength, bufferPosition - startPosition);
                            line = newLine;
                            lineLength = newLineLength;
                        }

                        while (!hasLine)
                        {
                            while (bufferPosition < bufferLength && ((qualifierCount > 0 && qualifierCount % 2 != 0) || (bPtr[bufferPosition] != '\r' && bPtr[bufferPosition] != '\n')))
                            {
                                if (bPtr[bufferPosition] == _textQualifier) qualifierCount++;
                                bufferPosition++;
                            }

                            if (bufferPosition == bufferLength)
                            {
                                appendToLine();
                                if (_reader.EndOfStream)
                                {
                                    break;
                                }
                                startPosition = 0;
                                bufferPosition = 0;
                                bufferLength = _reader.ReadBlock(cBuff, 0, _readBufferSize);
                            }
                            else
                            {
                                if (bufferPosition > startPosition + 1)
                                {
                                    appendToLine();
                                    hasLine = true;
                                }
                                bufferPosition++;
                                startPosition = bufferPosition;
                            }
                        }

                        counter++;
                        try
                        {
                            var columns = this.SplitIntoColumns(line, lineLength);
							while (_bufferSizeRows != 0 && _buffer.Count > _bufferSizeRows)
							{
								Thread.Sleep(50);
							}

							_buffer.Enqueue(new DataRow { Columns = columns, RowNumber = counter });
						} catch(Exception ex){
                            DataMoverLog.ErrorAsync($"Row {counter} throws: {ex.Message}" );
                            DataMoverLog.DebugAsync(new StringBuilder().Append(line).ToString());
                        }

                        if (stopwatch.Elapsed.TotalSeconds - lastelapsed >= 10)
                        {
                            DataMoverLog.DebugAsync($"Loaded {counter} lines in {stopwatch.Elapsed.TotalSeconds} seconds");
                            lastelapsed = stopwatch.Elapsed.TotalSeconds;
                        }
                    }

                    stopwatch.Stop();
                    DataMoverLog.DebugAsync($"Loaded {counter} lines in {stopwatch.Elapsed.TotalSeconds} seconds.");
                    _eof = true;
                }
            }catch(Exception ex){
				_eof = true;
				DataMoverLog.ErrorAsync(ex.Message);
            }
        }

        private unsafe string[] SplitIntoColumns(char[] source, int length)
        {
            var result = new List<string>();
            if (source == null || length == 0)
            {
                return new string[0];
            }
            fixed (char* line = source)
            {
                var index = 0;
                while (index < length)
                {
                    var expected = _delimiter;
                    if (line[index] == _delimiter)
                    {
                        index++;
                        result.Add(null);
                        continue;
                    }
                    int start, len;
                    if (line[index] == _textQualifier)
                    {
                        expected = _textQualifier;
                        start = index + 1;
                    }
                    else
                    {
                        start = index;
                    }

                    len = 0;
                    var done = false;
                    while (!done)
                    {
                        index++;
                        var idx = index;
                        while (idx < length && line[idx] != expected) idx++;

                        len = idx - start;
                        index = idx;

                        if (index < length)
                        {
                            if (index < length - 1 && source[index] == _textQualifier)
                            {
                                if (source[index + 1] == _textQualifier)
                                {
                                    index++;
                                    continue;
                                }
                                if (source[index + 1] == _delimiter)
                                {
                                    index++;
                                    index++;
                                    done = true;
                                }
                                else
                                {
                                    throw new FormatException("Row in incorrect format");
                                }
                            }
                            else
                            {
                                done = true;
                                index++;
                            }
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    var resultLine = new char[len];
                    fixed (char* resultPtr = resultLine)
                    {
                        var idx = 0;
                        var end = start + len;
                        for (var i = start; i < end; i++)
                        {
                            if (line[i] == _textQualifier)
                            {
                                i++;
                                if (i < end)
                                {
                                    resultPtr[idx++] = line[i];
                                }
                            }
                            else
                            {
                                resultPtr[idx++] = line[i];
                            }
                        }
                        result.Add(new string(resultPtr, 0, idx));
                    }
                }
            }
            return result.ToArray();
        }
    }
}