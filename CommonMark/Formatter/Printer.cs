﻿using CommonMark.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonMark.Formatter
{
    internal static class Printer
    {
        private static string format_str(string s)
        {
            int pos = 0;
            int len = s.Length;
            StringBuilder result = new StringBuilder();
            char c;
            result.Append("\"");
            while (pos < len)
            {
                c = s[pos];
                switch (c)
                {
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '"':
                        result.Append("\\\"");
                        break;
                    case '\\':
                        result.Append("\\\\");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
                pos++;
            }
            result.Append("\"");
            return result.ToString();
        }

        /// <summary>
        /// Write the block data to the given writer.
        /// </summary>
        public static void PrintBlocks(System.IO.TextWriter writer, Block block, int indent)
        {
            var stack = new Stack<BlockStackEntry>();

            while (block != null)
            {
                writer.Write(new string(' ', indent));

                switch (block.Tag)
                {
                    case BlockTag.Document:
                        writer.WriteLine("document");
                        break;

                    case BlockTag.BlockQuote:
                        writer.WriteLine("block_quote");
                        break;

                    case BlockTag.ListItem:
                        writer.WriteLine("list_item");
                        break;

                    case BlockTag.List:
                        var data = block.ListData;
                        if (data.ListType == ListType.Ordered)
                        {
                            writer.WriteLine("list (type=ordered tight={0} start={1} delim={2})",
                                 data.IsTight,
                                 data.Start,
                                 data.Delimiter);
                        }
                        else
                        {
                            writer.WriteLine("list (type=bullet tight={0} bullet_char={1})",
                                 data.IsTight,
                                 data.BulletChar);
                        }
                        break;

                    case BlockTag.AtxHeader:
                        writer.WriteLine("atx_header (level={0})", block.HeaderLevel);
                        print_inlines(writer, block.InlineContent, indent + 2, 0);
                        break;

                    case BlockTag.SETextHeader:
                        writer.WriteLine("setext_header (level={0})", block.HeaderLevel);
                        print_inlines(writer, block.InlineContent, indent + 2, 0);
                        break;

                    case BlockTag.Paragraph:
                        writer.WriteLine("paragraph");
                        print_inlines(writer, block.InlineContent, indent + 2, 0);
                        break;

                    case BlockTag.HorizontalRuler:
                        writer.WriteLine("hrule");
                        break;

                    case BlockTag.IndentedCode:
                        writer.WriteLine("indented_code {0}", format_str(block.StringContent.ToString()));
                        break;

                    case BlockTag.FencedCode:
                        writer.WriteLine("fenced_code length={0} info={1} {2}",
                               block.FencedCodeData.FenceLength,
                               format_str(block.FencedCodeData.Info),
                               format_str(block.StringContent.ToString()));
                        break;

                    case BlockTag.HtmlBlock:
                        writer.WriteLine("html_block {0}", format_str(block.StringContent.ToString()));
                        break;

                    case BlockTag.ReferenceDefinition:
                        writer.WriteLine("reference_def");
                        break;

                    default:
                        throw new CommonMarkException("Block type " + block.Tag + " is not supported.", block);
                }

                if (block.FirstChild != null)
                {
                    if (block.NextSibling != null)
                        stack.Push(new BlockStackEntry(indent, block.NextSibling));

                    indent += 2;
                    block = block.FirstChild;
                }
                else if (block.NextSibling != null)
                {
                    block = block.NextSibling;
                }
                else if (stack.Count > 0)
                {
                    var entry = stack.Pop();
                    indent = entry.Indent;
                    block = entry.Target;
                }
                else
                {
                    block = null;
                }
            }
        }

        // Prettyprint an inline list, for debugging.
        public static void print_inlines(System.IO.TextWriter writer, Inline ils, int indent, int depth)
        {
            if (depth > 100)
                throw new CommonMarkException("The document contains inline elements nested more than 100 levels deep which is not supported.");

            while (ils != null)
            {
                /*
                // we add 11 extra spaces for the line/column info
                for (int i=0; i < 11; i++) {
                  putchar(' ');
                }
                putchar('|');
                putchar(' ');
                */
                for (int i = 0; i < indent; i++)
                {
                    writer.Write(' ');
                }
                switch (ils.Tag)
                {
                    case InlineTag.String:
                        writer.WriteLine("str {0}", format_str(ils.LiteralContent));
                        break;
                    case InlineTag.LineBreak:
                        writer.WriteLine("linebreak");
                        break;
                    case InlineTag.SoftBreak:
                        writer.WriteLine("softbreak");
                        break;
                    case InlineTag.Code:
                        writer.WriteLine("code {0}", format_str(ils.LiteralContent));
                        break;
                    case InlineTag.RawHtml:
                        writer.WriteLine("html {0}", format_str(ils.LiteralContent));
                        break;
                    case InlineTag.Link:
                        writer.WriteLine("link url={0} title={1}",
                               format_str(ils.Linkable.Url),
                               format_str(ils.Linkable.Title));
                        print_inlines(writer, ils.Linkable.Label, indent + 2, depth + 1);
                        break;
                    case InlineTag.Image:
                        writer.WriteLine("image url={0} title={1}",
                               format_str(ils.Linkable.Url),
                               format_str(ils.Linkable.Title));
                        print_inlines(writer, ils.Linkable.Label, indent + 2, depth + 1);
                        break;
                    case InlineTag.Strong:
                        writer.WriteLine("strong");
                        print_inlines(writer, ils.Linkable.Label, indent + 2, depth + 1);
                        break;
                    case InlineTag.Emphasis:
                        writer.WriteLine("emph");
                        print_inlines(writer, ils.Linkable.Label, indent + 2, depth + 1);
                        break;
                }
                ils = ils.NextSibling;
            }
        }

        private struct BlockStackEntry
        {
            public readonly int Indent;
            public readonly Block Target;
            public BlockStackEntry(int indent, Block target)
            {
                this.Indent = indent;
                this.Target = target;
            }
        }
    }
}
