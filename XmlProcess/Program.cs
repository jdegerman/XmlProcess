using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace XmlProcess
{
    class Program
    {
        enum ExitCode
        {
            Success = 0,
            Error = 1
        }
        static int Main(string[] args)
        {
            args = new[] { "--vars", "Samples\\variables.xvar", "--input", "Samples\\input.xtm", "--target", "dev", "--output", "Samples\\output.xml"};
            try
            {
                var commmand_line_args = ReadCommandLineArguments(args);
                if(commmand_line_args.ContainsKey("vars"))
                {
                    if (!File.Exists(commmand_line_args["vars"]))
                        throw new Exception("Specified variable file not found");
                    if (!commmand_line_args.ContainsKey("target"))
                        throw new Exception("Target must be specified when using a variable file");
                }
                if (!File.Exists(commmand_line_args["input"]))
                    throw new Exception("Specified input file not found");
                var env_vars = new Dictionary<string, string>();
                if(commmand_line_args.ContainsKey("vars"))
                {
                    using (var var_reader = new StreamReader(commmand_line_args["vars"]))
                    {
                        var env_settings = EnvironmentSettings.LoadXml(ConvertToXml(var_reader));
                        env_vars = env_settings.GetByTarget(commmand_line_args["target"]);
                    }
                }
                string doc_xml;
                using (var file_reader = new StreamReader(commmand_line_args["input"]))
                {
                    doc_xml = ConvertToXml(file_reader);
                    doc_xml = ExtractVariables(env_vars, doc_xml);
                }
                if (commmand_line_args.ContainsKey("output"))
                    File.WriteAllText(commmand_line_args["output"], doc_xml);
                else
                    Console.WriteLine(doc_xml);
                return (int)ExitCode.Success;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Usage: XmlProcess --input <infile> --output <outfile> [--vars <varfile> --target <target>]");
                return (int)ExitCode.Error;
            }
        }
        private static Dictionary<string, string> ReadCommandLineArguments(string[] args)
        {
            var mandatory_keys = new[] { "input" };
            var command_line_arguments = new Dictionary<string, string>();
            for (var i = 0; i < args.Length; i += 2)
            {
                string name, value;
                args.TryGet(i, out name);
                if (!name.StartsWith("--"))
                    throw new Exception("Invalid commmand line argument '" + name + "'");
                if (name == "--")
                    throw new Exception("Invalid command line argument '--'");
                if (!args.TryGet(i + 1, out value))
                    throw new Exception("Command line argument '" + name + "' has no value associated");
                name = name.Substring(2);
                if(command_line_arguments.ContainsKey(name))
                    throw new Exception("Command line argument '" + name + "' has multiple definitions");
                command_line_arguments.Add(name, Environment.ExpandEnvironmentVariables(value));
            }
            foreach(var mandatory_key in mandatory_keys)
            {
                if (!command_line_arguments.ContainsKey(mandatory_key))
                    throw new Exception("Mandatory command line argument '" + mandatory_key + "' has not been defined");
            }
            return command_line_arguments;
        }
        private static string ExtractVariables(Dictionary<string, string> env_vars, string doc_xml)
        {
            doc_xml = doc_xml.Replace("{(.+?)}", m => 
            {
                var parts = m.Groups[1].Value.Split(' ');
                var key = parts[0];
                var args = parts.Tail();
                if (key == "guid")
                {
                    return Guid.NewGuid().ToString();
                }
                else if (key == "datetime")
                {
                    if (args.Length == 0)
                        return DateTime.Now.ToString();
                    else
                        return DateTime.Now.ToString(string.Join(" ", args));
                }
                return env_vars.GetValue(m.Groups[1].Value);
            });
            return doc_xml;
        }
        static string ConvertToXml(TextReader input)
        {
            Tokenizer.TokenizeInput(input);
            var xml = Element();
            return prettyPrintXml(xml);
        }
        static string prettyPrintXml(string xml)
        {
            using (var ms = new MemoryStream())
            using (var writer = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented })
            using (var reader = new StreamReader(ms))
            {
                var xdoc = new XmlDocument();
                xdoc.LoadXml(xml);
                xdoc.WriteContentTo(writer);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return reader.ReadToEnd();
            }
        }
        static string Element()
        {
            Token t;
            if (Tokenizer.MatchPeek(Symbols.STRING, out t))
            {
                return GetStringContent(t);
            }
            var ident = Tokenizer.Require(Symbols.IDENT, "Expected valid element identifier").Value;
            var xml = new StringBuilder(Symbols.START_ELEMENT + ident);
            xml.Append(GetXmlAttributes());

            if (Tokenizer.MatchPeek(Symbols.END_STMT, out t))
            {
                Tokenizer.Require(Symbols.END_STMT, "");
                return xml + Symbols.END_START_ELEMENT;
            }
            else
            {
                xml.Append(Symbols.END_ELEMENT);
                Tokenizer.Require(Symbols.L_BRACE, "Expected { or ;");
                while (!Tokenizer.MatchPeek(Symbols.R_BRACE, out t))
                    xml.Append(Element());
                Tokenizer.Require(Symbols.R_BRACE, "");
                return xml + Symbols.START_CLOSE_ELEMENT + ident + Symbols.END_ELEMENT;
            }
        }

        private static string GetXmlAttributes()
        {
            Token t;
            var xml = string.Empty;
            if (Tokenizer.MatchPeek(Symbols.L_PARA, out t))
            {
                Tokenizer.Require(Symbols.L_PARA, "");
                while (!Tokenizer.MatchPeek(Symbols.R_PARA, out t))
                {
                    var attributeName = Tokenizer.Require(Symbols.IDENT, "Expected valid attribute identifier");
                    var attributeValue = Tokenizer.Require(Symbols.ANY, "");
                    xml += string.Format(" {0}=\"{1}\"", attributeName.Value, attributeValue.Value.Trim('\'', '"'));
                }
                Tokenizer.Require(Symbols.R_PARA, "");
            }
            return xml;
        }

        private static string GetStringContent(Token t)
        {
            Tokenizer.Require(Symbols.STRING, "");
            return t.Value.Trim('\'', '"');
        }
    }
}
