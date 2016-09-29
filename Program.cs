using GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TestScientist
{
    class Program
    {
        public static bool IsFeatureEnabled { get; private set; }

        static void Main(string[] args)
        {
            IsFeatureEnabled = true;
            Scientist.ResultPublisher = new MyResultPublisher();
            Console.WriteLine("User:" + FormatList(GetUsers()));
            Console.ReadLine();
        }

        public static string FormatList(List<string> list)
        {
            return string.Join(",", list);
        }

        private static List<string> GetUsers()
        {
            return Scientist.Science<List<string>>("get users", experiment =>
            {
                experiment.RunIf(() => IsFeatureEnabled);
                experiment.Compare((x, y) => x.SequenceEqual(y));

                experiment.Use(() => { return new List<string> { "Pierre", "Jean" }; }); // old way
                experiment.Try(() => { return new List<string> { "Pierre", "John" }; }); // new way
            });
        }
    }

    public class MyResultPublisher : IResultPublisher
    {
        public Task Publish<T, TClean>(Result<T, TClean> result)
        {
            Console.WriteLine($"Publishing results for experiment '{result.ExperimentName}'");
            Console.WriteLine($"Result: {(result.Matched ? "MATCH" : "MISMATCH")}");
            Console.WriteLine($"Control value: {new JavaScriptSerializer().Serialize(result.Control.Value)}");
            Console.WriteLine($"Control duration: {result.Control.Duration}");
            foreach (var observation in result.Candidates)
            {
                Console.WriteLine($"Candidate name: {observation.Name}");
                Console.WriteLine($"Candidate value: {new JavaScriptSerializer().Serialize(observation.Value)}");
                Console.WriteLine($"Candidate duration: {observation.Duration}");
            }

            if (result.Mismatched)
            {
                // saved mismatched experiments to DB
                //DbHelpers.SaveExperimentResults(result);
            }

            return Task.FromResult(0);
        }
    }

}
