using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace ClusterCreator
{
    // Define a class to receive parsed values
    class Options
    {
        [Option('f', "rc", Required = true,
          HelpText = "RC file name to be processed.")]
        public string rcFileName { get; set; }

        [Option('o', "osPassword", Required = false,
          HelpText = "OS password.")]
        public string osPassword { get; set; }

        [Option('k', "key", Required = true,
          HelpText = "Keypair name.")]
        public string keyPairName { get; set; }

        [Option('e', "email", Required = true,
          HelpText = "Admin email for cluster.")]
        public string eMail { get; set; }

        [Option('p', "password", Required = true,
          HelpText = "Admin password for cluster.")]
        public string password { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            StreamReader rcFile;
            string OS_AUTH_URL;
            string OS_TENANT_ID;
            string OS_TENANT_NAME;
            string OS_USERNAME;
            string OS_REGION_NAME;

            string createCluster = @"cf-mgmt.exe ^
    --os-auth-url {0} ^
    --os-username {1} ^
    --os-password {2} ^
    --os-tenant-id {3} ^
    --os-tenant-name {4} ^
    --os-region-name {5} ^
create-cluster ^
    --keypair-name {6} ^
    --admin-email {7} ^
    --admin-password {8} ^
    --load http://clients.als.hpcloud.com/1.2/config/trial.yml";

            string addWinDEA = @"cf-mgmt.exe ^
    --os-auth-url {0} ^
    --os-username {1} ^
    --os-password {2} ^
    --os-tenant-id {3} ^
    --os-tenant-name {4} ^
    --os-region-name {5} ^
add-role dea ^
    --keypair-name {6} ^
    --load http://clients.als.hpcloud.com/1.2/config/trial-windea.yml";

            string addMSSQL = @"cf-mgmt.exe ^
    --os-auth-url {0} ^
    --os-username {1} ^
    --os-password {2} ^
    --os-tenant-id {3} ^
    --os-tenant-name {4} ^
    --os-region-name {5} ^
add-service mssql2014 ^
    --keypair-name 1_2_key ^
    --load http://clients.als.hpcloud.com/1.2/config/trial-mssql2014.yml";

            Options options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            try
            {
                rcFile = new StreamReader(options.rcFileName);//rcFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to read file: " + options.rcFileName);
                return;
            }
            string contents = rcFile.ReadToEnd();
            int index;
            int sentenceIndex;
            //find OS_AUTH_URL
            index = contents.IndexOf("OS_AUTH_URL");
            sentenceIndex = contents.IndexOf('\n', index++);
            index += "OS_AUTH_URL".Length;
            OS_AUTH_URL = contents.Substring(index, sentenceIndex - index);
            //find OS_TENANT_ID
            index = contents.IndexOf("OS_TENANT_ID");
            sentenceIndex = contents.IndexOf('\n', index++);
            index += "OS_TENANT_ID".Length;
            OS_TENANT_ID = contents.Substring(index, sentenceIndex - index);
            //find OS_TENANT_NAME
            index = contents.IndexOf("OS_TENANT_NAME");
            sentenceIndex = contents.IndexOf('\n', index++);
            index += "OS_TENANT_NAME".Length;
            OS_TENANT_NAME = contents.Substring(index, sentenceIndex - index);
            //find OS_USERNAME
            index = contents.IndexOf("OS_USERNAME");
            sentenceIndex = contents.IndexOf('\n', index++);
            index += "OS_USERNAME".Length;
            OS_USERNAME = contents.Substring(index, sentenceIndex - index);
            //find OS_REGION_NAME
            index = contents.IndexOf("OS_REGION_NAME=");
            sentenceIndex = contents.IndexOf('\n', index++);
            index += "OS_REGION_NAME".Length;
            OS_REGION_NAME = contents.Substring(index, sentenceIndex - index);


            string content = String.Format(createCluster, OS_AUTH_URL, OS_USERNAME, options.osPassword==null?"<enter OS password>":options.osPassword, OS_TENANT_ID, OS_TENANT_NAME, OS_REGION_NAME, options.keyPairName, options.eMail, options.password);
            StreamWriter sw = new StreamWriter("createCluster.bat");
            sw.Write(content);
            sw.Close();

            content = String.Format(addWinDEA, OS_AUTH_URL, OS_USERNAME, options.osPassword == null ? "<enter OS password>" : options.osPassword, OS_TENANT_ID, OS_TENANT_NAME, OS_REGION_NAME, options.keyPairName);
            sw = new StreamWriter("addWinDEA.bat");
            sw.Write(content);
            sw.Close();

            content = String.Format(addMSSQL, OS_AUTH_URL, OS_USERNAME, options.osPassword == null ? "<enter OS password>" : options.osPassword, OS_TENANT_ID, OS_TENANT_NAME, OS_REGION_NAME, options.keyPairName);
            sw = new StreamWriter("addMSSQLService.bat");
            sw.Write(content);
            sw.Close();            

            Console.WriteLine("Cluster creation files written");
            Console.WriteLine("Run the files in the following order in a command line that has cf-mgmt access");
            if(options.osPassword == null)
            {
                Console.WriteLine("OS Password was not provided ensure to modify the file before running the files.");
            }
            Console.WriteLine("1. createCluster.bat");
            Console.WriteLine("2. addWinDEA.bat");
            Console.WriteLine("3. addMSSQLService.bat");

        }
    }
}
