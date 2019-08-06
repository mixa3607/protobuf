using System;
using System.Collections.Generic;
using System.IO;

namespace SilentOrbit.ProtocolBuffers
{
    public static class Program
    {
        public static void Main()
        {
            var opts = new Options {InputProto = new List<string>() {"file.proto"},
                OutputPath = @"C:\path",
                BaseNamespace = "Test.Space",
                GenerateForProtoDotNet = true,
                UseArrays = true,
                NoProtocolParser = true,
                GenerateFields = true,
                UseFullTypeName = true,
                OneClassOneFile = true,
                NoGenerateSerializer = true
            };
            Build(opts);
            
        }
        public static void Build(Options options)
        {
            var parser = new FileParser();
            var collection = parser.Import(options.InputProto);

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
