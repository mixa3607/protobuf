using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleTranslateFreeApi;

namespace SilentOrbit.ProtocolBuffers
{
    public static class Program
    {
        public static void Main()
        {
            var opts = new Options {
                InputProto = new List<string> {"p.proto"},
                OutputPath = @"./P",
                BaseNamespace = "Test",
                GenerateForProtoDotNet = true,
                UseArrays = true,
                NoProtocolParser = true,
                GenerateFields = true,
                UseFullTypeName = true,
                OneClassOneFile = true,
                NoGenerateSerializer = true,
                FixComments = true,
                TranslateComments = true,
                FromLanguage = Language.Auto,
                ToLanguage = Language.English
            };
            Build(opts);
            
        }
        public static void Build(Options options)
        {
            var parser = new FileParser();
            var collection = parser.Import(options.InputProto, options.FixComments);

            Console.WriteLine(collection);

            //Interpret and reformat
            try
            {
                var pp = new ProtoPrepare(options);
                pp.Prepare(collection);
            }
            catch (ProtoFormatException pfe)
            {
                Console.WriteLine();
                Console.WriteLine(pfe.SourcePath.Path + "(" + pfe.SourcePath.Line + "," + pfe.SourcePath.Column + "): error CS001: " + pfe.Message);
                throw;
            }


            if (options.TranslateComments)
            {
                var translator = new GoogleTranslator();
                if (!string.IsNullOrWhiteSpace(collection.Comments))
                {
                    collection.Comments = translator.TranslateAsync(collection.Comments, options.FromLanguage, options.ToLanguage).GetAwaiter().GetResult().MergedTranslation;
                    //Console.WriteLine(collection.Comments);
                }
            
                foreach (var message in collection.Messages)
                {
                    message.Value.Comments = translator.TranslateAsync(message.Value.Comments, options.FromLanguage, options.ToLanguage).GetAwaiter().GetResult().MergedTranslation;
                    Console.WriteLine(message.Value.Comments);
                    foreach (var valueField in message.Value.Fields)
                    {
                        valueField.Value.Comments = translator.TranslateAsync(valueField.Value.Comments, options.FromLanguage, options.ToLanguage).GetAwaiter().GetResult().MergedTranslation;
                        Console.WriteLine(valueField.Value.Comments);
                    }
                }
            }



            //Generate code
            if (options.OneClassOneFile)
            {
                
                var classes = collection.Messages;
                var path = options.OutputPath;
                foreach (var protoMessage in classes)
                {
                    collection.Messages = new Dictionary<string, ProtoMessage>()
                        {{protoMessage.Key, protoMessage.Value}};
                    options.OutputPath = Path.Combine(path, protoMessage.Value.CsType + ".cs");
                    ProtoCode.Save(collection, options);
                }
            }
            else
            {
                ProtoCode.Save(collection, options);
                Console.WriteLine("Saved: " + options.OutputPath);
            }
        }
    }
}
