﻿using System;
using System.CodeDom.Compiler;

namespace PricingCalc.Model.Generators
{
    internal static class IndentedTextWriterExtensions
    {
        public static void Preambula(this IndentedTextWriter code)
        {
            code.WriteLine(@"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
");

            code.WriteLine("#nullable enable");
            code.EmptyLine();
        }

        public static void Block(this IndentedTextWriter code, Action action, bool addSemicolon = false)
        {
            code.WriteLine("{");
            code.Indent++;

            action();

            code.Indent--;
            if (!addSemicolon)
            {
                code.WriteLine("}");
            }
            else
            {
                code.WriteLine("};");
            }
        }

        public static void Interface(this IndentedTextWriter code, bool isInternal, string name, string[] bases, Action body)
        {
            code.WriteLine($"{Visibility(isInternal)} interface {name} : {string.Join(", ", bases)}");
            code.Block(() =>
            {
                body();
            });
        }

        public static void Class(this IndentedTextWriter code, string attributes, string name, string[] bases, Action body)
        {
            code.WriteLine($"internal {attributes} class {name} : {string.Join(", ", bases)}");
            code.Block(() =>
            {
                body();
            });
        }

        public static void EmptyLine(this IndentedTextWriter code)
        {
            code.WriteLineNoTabs(string.Empty);
        }

        public static void GeneratedClassAttributes(this IndentedTextWriter code)
        {
            code.GeneratedCodeAttribute();
            code.CompilerGeneratedCodeAttribute();
            code.DebuggerNonUserCodeAttribute();
        }

        public static void GeneratedInterfaceAttributes(this IndentedTextWriter code)
        {
            code.GeneratedCodeAttribute();
            code.CompilerGeneratedCodeAttribute();
        }

        public static void GeneratedCodeAttribute(this IndentedTextWriter code)
        {
            code.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"C# Source Generator\", \"{typeof(GeneratorBase).Assembly.GetName().Version}\")]");
        }

        public static void CompilerGeneratedCodeAttribute(this IndentedTextWriter code)
        {
            code.WriteLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
        }

        public static void DebuggerNonUserCodeAttribute(this IndentedTextWriter code)
        {
            code.WriteLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
        }

        private static string Visibility(bool isInternal)
        {
            return isInternal ? "internal" : "public";
        }
    }
}
