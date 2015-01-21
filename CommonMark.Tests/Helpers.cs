﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonMark.Tests
{
    internal static class Helpers
    {
        public static Syntax.Block ParseDocument(string commonMark, CommonMarkSettings settings = null)
        {
            using (var reader = new System.IO.StringReader(Helpers.Normalize(commonMark)))
            {
                var doc = CommonMarkConverter.ProcessStage1(reader, settings);
                CommonMarkConverter.ProcessStage2(doc, settings);
                return doc;
            }
        }

        public static void ExecuteTest(string commonMark, string html, CommonMarkSettings settings = null)
        {
            Helpers.LogValue("CommonMark", Denormalize(commonMark));
            Helpers.LogValue("Expected", Denormalize(html));

            // Arrange
            commonMark = Helpers.Normalize(commonMark);
            html = Helpers.Normalize(html);

            // Act
            var actual = CommonMarkConverter.Convert(commonMark, settings);

            // Assert
            Helpers.LogValue("Actual", Denormalize(actual));
            Assert.AreEqual(Helpers.Tidy(html), Helpers.Tidy(actual));
        }

        private static string Denormalize(string value)
        {
            value = value.Replace('\t', '→');
            value = value.Replace(' ', '␣');
            return value;
        }

        private static string Normalize(string value)
        {
            value = value.Replace('→', '\t');
            value = value.Replace('␣', ' ');
            return value;
        }

        public static string Tidy(string html)
        {
            html = html.Replace("\r", "").Trim();

            // collapse spaces and newlines before </li> and after <li>
            html = Regex.Replace(html, @"\s+</li>", "</li>");
            html = Regex.Replace(html, @"<li>\s+", "<li>");

            // needed to compare UTF-32 characters
            html = html.Normalize(NormalizationForm.FormKD);
            return html;
        }

        public static void Log(string format, params object[] args)
        {
            if (args != null && args.Length > 0)
                Console.WriteLine(format, args);
            else
                Console.WriteLine(format);
        }

        public static void LogValue(string caption, string text)
        {
            Console.Write(caption);
            Console.WriteLine(":");
            Console.WriteLine();

            Console.Write("    ");
            Console.WriteLine(text.Replace("\n", "\n    "));

            Console.WriteLine();
        }
    }
}
