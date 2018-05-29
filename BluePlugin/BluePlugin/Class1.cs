﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using PluginContracts;

namespace BluePlugin
{
    public class BluePlugin : IPlugin
    {
        public static string[] words = { "public", "private", "get", "set" };
        public string Name
        {
            get;
        }

        TextRange FindWordFromPosition(TextPointer position, string word)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun);
                        TextPointer end = start.GetPositionAtOffset(word.Length);

                        TextRange test = new TextRange(end, end.GetPositionAtOffset(1));

                        if (word == "for")
                        {
                            if (test.Text != "e")
                                return new TextRange(start, end);
                            else
                                continue;
                        }
                        else
                        {
                            if (test.Text == " " || test.Text == "" || test.Text == "\n" || test.Text == "(")
                                return new TextRange(start, end);
                            else
                                continue;
                        }

                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "word" is not found.
            return null;
        }

        public void Do(System.Windows.Controls.RichTextBox rtb)
        {
            foreach (string key in words)
            {
                TextPointer pointer = rtb.Document.ContentStart;
                TextRange range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                while (range != null)
                {
                    range = FindWordFromPosition(pointer, key);
                    if (range == null)
                        break;
                    if (range.GetPropertyValue(TextElement.ForegroundProperty) == Brushes.Blue)
                    {
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    }
                    else
                    {
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    }

                    if (range != null)
                        pointer = range.End;
                }
            }
        }
    }
}
