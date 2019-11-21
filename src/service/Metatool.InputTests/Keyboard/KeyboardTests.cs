using Microsoft.VisualStudio.TestTools.UnitTesting;
using Metatool.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Metatool.Input.Tests
{
    [TestClass()]
    public class KeyboardTests
    {
        [TestMethod()]
        public void ReplaceAliasTest()
        {
            // var input = "` CAPS` + ` b` +`,+ ,`";
            // var key = "`";
            // var regex = @$"(?:(?<=[\s,+])|^){Regex.Escape(key)}(?:(?=[\s,+])|$)";

            var input = ", ,, ,, ,";
            var key   = "`";
            var regex = @$",\s*,";
            var b = Regex.Replace(input, regex, ", W");
               

            // (?<=[\s,+])`
        }
    }
}