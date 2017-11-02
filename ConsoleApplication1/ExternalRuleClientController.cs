using System;
using System.Xml;
using System.IO;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Activities.Rules;
using System.Data.SqlClient;
using System.Configuration;
using System.Workflow.ComponentModel.Compiler;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class ExternalRuleClientController
    {
        string _ruleSetName;
        private string RuleSetName;
        private string _connectionString;
        WorkflowMarkupSerializer _serializer;
        RuleSet _ruleSet;

        public void LoadRuleSet(string ruleSetName)
        {
            _serializer = new WorkflowMarkupSerializer();
            _ruleSet = GetRuleSetFromDB(_serializer);
            if (string.IsNullOrEmpty(ruleSetName))
                throw new Exception("Ruleset name cannot be null or empty.");
            if (!string.Equals(RuleSetName, ruleSetName))
            {
                _ruleSetName = ruleSetName;

                if (_ruleSet == null)
                {
                    throw new Exception("RuleSet could not be loaded. Make sure the connection string and ruleset name are correct.");
                }
            }
        }

        private RuleSet GetRuleSetFromDB(WorkflowMarkupSerializer serializer)
        {
            SqlDataReader reader;
            SqlConnection sqlConn = null;
            ConnectionStringSettingsCollection connectionStringSettingsCollection = ConfigurationManager.ConnectionStrings;
            RuleSet resultRuleSet = null;
            foreach (ConnectionStringSettings connectionStringSettings in connectionStringSettingsCollection)
            {
                if (string.CompareOrdinal(connectionStringSettings.Name, "RuleSetStoreConnectionString") == 0)
                    _connectionString = connectionStringSettings.ConnectionString;
            }
            if (string.IsNullOrEmpty(_connectionString))
                return null;

            try
            {
                sqlConn = new SqlConnection(_connectionString);
                sqlConn.Open();
                SqlParameter p1 = new SqlParameter("@p1", "RuleSet1");
                string commandString = "SELECT * FROM RuleSet WHERE Name= @p1 ORDER BY ModifiedDate DESC";
                SqlCommand command = new SqlCommand(commandString, sqlConn);
                command.Parameters.Add(p1);
                reader = command.ExecuteReader();
                reader.Read();
                resultRuleSet = DeserializeRuleSet(reader.GetString(3), serializer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            sqlConn.Close();
            sqlConn.Dispose();
            return resultRuleSet;
        }

        public static RuleSet LoadRules(string rulesFileName)

        {

            if (File.Exists(rulesFileName))

            {

                FileStream fs = new FileStream(rulesFileName, FileMode.Open);

                StreamReader sr = new StreamReader(fs);

                string serializedRuleSet = sr.ReadToEnd();

                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

                StringReader stringReader = new StringReader(serializedRuleSet);

                XmlTextReader reader = new XmlTextReader(stringReader);

                var ruleSet = serializer.Deserialize(reader) as RuleSet;

                fs.Close();



                return ruleSet;

            }

            return null;

        }

        private RuleSet DeserializeRuleSet(string ruleSetXmlDefinition, WorkflowMarkupSerializer serializer)
        {
            WorkflowMarkupSerializer s = new WorkflowMarkupSerializer();
            StringReader stringReader = new StringReader(ruleSetXmlDefinition);
            XmlTextReader reader = new XmlTextReader(stringReader);
            return s.Deserialize(reader) as RuleSet;
        }

        private RuleExecution ValidateRuleSet(object targetObject)
        {
            RuleValidation ruleValidation;

            ruleValidation = new RuleValidation(targetObject.GetType(), null);
            if (!_ruleSet.Validate(ruleValidation))
            {
                string errors = "";
                foreach (ValidationError validationError in ruleValidation.Errors)
                    errors = errors + validationError.ErrorText + "\n";
                Debug.WriteLine("Validation Errors \n" + errors);
                return null;
            }
            else
            {
                return new RuleExecution(ruleValidation, targetObject);
            }
        }

        private void ExecuteRule(RuleExecution ruleExecution)
        {
            if (null != ruleExecution)
            {
                _ruleSet.Execute(ruleExecution);
            }
            else
            {
                throw new Exception("RuleExecution is null.");
            }
        }

        public void ExecuteRuleSet(object targetObject)
        {
            if (_ruleSet != null)
            {
                RuleExecution ruleExecution;
                ruleExecution = ValidateRuleSet(targetObject);
                ExecuteRule(ruleExecution);
            }
        }
    }

}
