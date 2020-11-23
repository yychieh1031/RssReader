using System;
using System.Text.RegularExpressions;

namespace lib
{
    public class YCLib
    {
        //
        // Title String Handler
        //
        static public string stringHandler(String inputString)
        {
            String ruleOne = @"(\s|\||\-)+";
            inputString = Regex.Replace(inputString, ruleOne, "_");
            inputString = inputString.Replace("'","");

            return inputString;
        }
    }   
}