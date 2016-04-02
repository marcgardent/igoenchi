using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.GoGame.UnitTest
{
    internal static class CompareSgfHelper
    {
        
             
        public static void CompareNode(SGFTree excepted, SGFTree actual, string indent)
        {
            Trace.WriteLine($"{indent}X");
            CompareProp(excepted, actual, indent);

            while (excepted.ChildNodes.Count == actual.ChildNodes.Count && excepted.ChildNodes.Count == 1)
            {
                excepted = excepted.ChildNodes[0];
                actual = actual.ChildNodes[0];
                CompareProp(excepted, actual, indent);
            }

            for (var i = 1; i < excepted.ChildNodes.Count && i < actual.ChildNodes.Count; i++)
            {
                CompareNode(excepted.ChildNodes[i], actual.ChildNodes[i], string.Concat(indent, "\t"));
            }

            if (excepted.ChildNodes.Count != actual.ChildNodes.Count)
            {
                Trace.WriteLine($"{indent}Children skipped : excepted.child='{excepted.ChildNodes.Count}' actual.child='{actual.ChildNodes.Count}'");
            }
        }

        private static string Censure(string target)
        {
            return target.Length < 30 ? target : string.Concat(target.Substring(0, 30), "(...)");
        }

        public static void CompareProp(SGFTree excepted, SGFTree actual, string indent)
        {

            var actualprop = actual.Properties.GroupBy(x => x.Name).ToList();
            var exceptedprop = excepted.Properties.GroupBy(x => x.Name);

            foreach (var grp in exceptedprop)
            {
                var actualgroup = actualprop.FirstOrDefault(x => x.Key == grp.Key);
                if (actualgroup == null)
                {
                    string values = SerializeValues(grp);
                    Trace.WriteLine($"{indent}missing property '{grp.Key}' = {values}");
                }
                else
                {
                    CompareValues(indent, actualprop, actualgroup, grp);
                }
            }

            foreach (var grp in actualprop)
            {
                var values = SerializeValues(grp);
                Trace.WriteLine($"{indent}new property '{grp.Key}' = {values}");
            }
        }

        private static void CompareValues(string indent, List<IGrouping<string, SGFProperty>> actualprop, IGrouping<string, SGFProperty> actualgroup, IGrouping<string, SGFProperty> grp)
        {
            actualprop.Remove(actualgroup);
            var actualValues = SerializeValues(actualgroup);
            var exceptedValues = SerializeValues(grp);
            if (actualValues != exceptedValues)
            {
                Trace.WriteLine($"{indent}Different Values of '{grp.Key}'");
                Trace.WriteLine($"{indent}\tOriginal : {Censure(exceptedValues)}");
                Trace.WriteLine($"{indent}\tSaveAs : {Censure(actualValues)}");
            }
        }

        private static string SerializeValues(IGrouping<string, SGFProperty> grp)
        {
            return string.Join(",", grp.Select(x => string.Concat("'", Censure(x.Value), "'")));
        }
    }
}
