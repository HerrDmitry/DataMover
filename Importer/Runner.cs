using System.IO;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;
using Importer.Configuration;

namespace Importer
{
    public static class Runner
    {
        public static void RunImport(string configurationFilePath)
        {
            var configuration =
                JsonConvert.DeserializeObject<Configuration.Configuration>(System.IO.File.OpenText(configurationFilePath)
                    .ReadToEnd());
            var context = configuration.GetContext();
            var sourceReader = context.GetDataReader();
            var dataWriter = context.GetDataWriter();
            while (dataWriter(sourceReader())) ;
            
            context.FinalizeImport();
        }
    }
}