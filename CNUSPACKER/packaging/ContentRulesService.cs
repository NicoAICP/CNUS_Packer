using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CNUSPACKER.contents;
using CNUSPACKER.fst;

namespace CNUSPACKER.packaging
{
    public static class ContentRulesService
    {
        private const long MAX_CONTENT_LENGTH = (long) (0xBFFFFFFFL * 0.975);

        private static long cur_content_size;

        private static Content cur_content;
        private static Content cur_content_first;

        public static void ApplyRules(FSTEntry root, Contents targetContents, List<ContentRule> rules)
        {
            Console.WriteLine("-----");
            foreach (ContentRule rule in rules)
            {
                Console.WriteLine("Apply rule \"" + rule.pattern + "\"");
                if (rule.contentPerMatch)
                {
                    SetNewContentRecursiveRule("", root, targetContents, rule);
                }
                else
                {
                    cur_content = targetContents.GetNewContent(rule.details);
                    cur_content_first = cur_content;
                    cur_content_size = 0L;
                    bool result = SetContentRecursiveRule("", root, targetContents, rule);
                    if (!result)
                    {
                        Console.WriteLine("No file matched the rule. Lets delete the content again");
                        targetContents.DeleteContent(cur_content);
                    }
                }
                Console.WriteLine("-----");
            }
        }

        private static Content SetNewContentRecursiveRule(string path, FSTEntry currentEntry, Contents targetContents, ContentRule rule)
        {
            path += currentEntry.filename + "/";
            Regex p = new Regex(rule.pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Content result = null;

            if (currentEntry.children.Count == 0)
            {
                Match m = p.Match(path);
                if (m.Success)
                {
                    result = targetContents.GetNewContent(rule.details);
                }
            }

            foreach (FSTEntry child in currentEntry.children)
            {
                if (child.isDir)
                {
                    result = SetNewContentRecursiveRule(path, child, targetContents, rule) ?? result;
                }
                else
                {
                    string childPath = path + child.filename;
                    Match m = p.Match(childPath);
                    if (m.Success)
                    {
                        Content result_content = targetContents.GetNewContent(rule.details);
                        Console.WriteLine($"Set content to {result_content.ID:X} for: {childPath}");
                        child.SetContent(result_content);
                        result = result_content;
                    }
                }
            }

            if (result != null)
                currentEntry.SetContent(result);

            return result;
        }

        private static bool SetContentRecursiveRule(string path, FSTEntry currentEntry, Contents targetContents, ContentRule rule)
        {
            path += currentEntry.filename + "/";
            Regex p = new Regex(rule.pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool matchFound = false;
            if (currentEntry.children.Count == 0)
            {
                Match m = p.Match(path);
                if (m.Success)
                {
                    Console.WriteLine($"Set content to {cur_content.ID:X} ({cur_content_size:X},{currentEntry.fileSize:X}) for: {path}");
                    currentEntry.SetContent(cur_content);

                    return true;
                }

                return false;
            }

            foreach (FSTEntry child in currentEntry.children)
            {
                if (child.isDir)
                {
                    matchFound |= SetContentRecursiveRule(path, child, targetContents, rule);
                }
                else
                {
                    string childPath = path + child.filename;
                    Match m = p.Match(childPath);
                    if (m.Success)
                    {
                        if (cur_content_size + child.fileSize > MAX_CONTENT_LENGTH)
                        {
                            Console.WriteLine($"Info: Target content size is bigger than {MAX_CONTENT_LENGTH} bytes. Content will be split into multiple files.");
                            cur_content = targetContents.GetNewContent(rule.details);
                            cur_content_size = 0;
                        }
                        cur_content_size += child.fileSize;

                        Console.WriteLine($"Set content to {cur_content.ID:X} ({cur_content_size:X},{child.fileSize:X}) for: {childPath}");
                        child.SetContent(cur_content);
                        matchFound = true;
                    }
                }
            }

            if (matchFound)
                currentEntry.SetContent(cur_content_first);

            return matchFound;
        }
    }
}
