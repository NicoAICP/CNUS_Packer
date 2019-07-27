using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CNUSPACKER.contents;
using CNUSPACKER.fst;

namespace CNUSPACKER.packaging
{
    public static class ContentRulesService
    {
        public const long MAX_CONTENT_LENGTH = (long) (0xBFFFFFFFL * 0.975);

        public static long cur_content_size;

        public static Content cur_content;
        public static Content cur_content_first;

        public static void ApplyRules(FSTEntry root, Contents targetContents, List<ContentRule> rules)
        {
            Console.WriteLine("-----");
            foreach (ContentRule rule in rules)
            {
                Console.WriteLine("Apply rule \"" + rule.pattern + "\"");
                if (rule.contentPerMatch)
                {
                    SetNewContentRecursiveRule("", rule.pattern, root, targetContents, rule);
                }
                else
                {
                    cur_content = targetContents.GetNewContent(rule.details);
                    cur_content_first = cur_content;
                    cur_content_size = 0L;
                    bool result = SetContentRecursiveRule("", rule.pattern, root, targetContents, rule.details);
                    if (!result)
                    {
                        Console.WriteLine("No file matched the rule. Lets delete the content again");
                        targetContents.DeleteContent(cur_content);
                    }
                    cur_content_first = null;
                }
                Console.WriteLine("-----");
            }
        }

        private static Content SetNewContentRecursiveRule(string path, string pattern, FSTEntry cur_entry, Contents targetContents, ContentRule rule)
        {
            path += cur_entry.filename + "/";
            Regex p =  new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Content result = null;

            if (cur_entry.children.Count == 0)
            {
                MatchCollection m = p.Matches(path);
                // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                if (m.Count > 0)
                {
                    Content result_content = targetContents.GetNewContent(rule.details);
                    result = result_content;
                }
            }
            foreach (FSTEntry child in cur_entry.children)
            {
                if (child.isDir)
                {
                    Content child_result = SetNewContentRecursiveRule(path, pattern, child, targetContents, rule);
                    if (child_result != null)
                    {
                        result = child_result;
                    }
                }
                else
                {
                    string filePath = path + child.filename;
                    Match m = p.Match(filePath);
                    // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                    if (m.Success)
                    {
                        Content result_content = targetContents.GetNewContent(rule.details);
                        if (!child.notInPackage) Console.WriteLine("Set content to " + result_content.ID.ToString("X") + " for: " + filePath);
                        child.SetContent(result_content);
                        result = result_content;
                    }
                }
            }
            if (result != null)
            {
                cur_entry.SetContent(result);
            }
            return result;
        }

        private static bool SetContentRecursiveRule(string path, string pattern, FSTEntry cur_entry, Contents targetContents, ContentDetails contentDetails)
        {
            path += cur_entry.filename + "/";
            Regex p = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool result = false;
            if (cur_entry.children.Count == 0)
            {
                Match m = p.Match(path);
                // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                if (m.Success)
                {
                    if (!cur_entry.notInPackage)
                        Console.WriteLine("Set content to " + cur_content.ID.ToString("X") + " (" + cur_content_size.ToString("X") + "," + cur_entry.GetFileSize().ToString("X") + ") for: " + path);
                    if (cur_entry.children.Count == 0/* && cur_entry.getFilename().equals("content")*/)
                    {  //TODO: may could cause problems. Current solution only apply to content folder.
                        cur_entry.SetContent(cur_content);
                    }

                    return true;
                }

                return false;
            }
            foreach (FSTEntry child in cur_entry.children)
            {
                if (child.isDir)
                {

                    bool child_result = SetContentRecursiveRule(path, pattern, child, targetContents, contentDetails);
                    if (child_result)
                    {
                        cur_entry.SetContent(cur_content_first);
                        result = true;
                    }
                }
                else
                {
                    string filePath = path + child.filename;
                    Match m = p.Match(filePath);
                    // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                    if (m.Success)
                    {
                        if (cur_content_size > 0 && cur_content_size + child.GetFileSize() > MAX_CONTENT_LENGTH)
                        {
                            Console.WriteLine("Info: Target content size is bigger than " + MAX_CONTENT_LENGTH + " bytes. Content will be split into multiple files. Don't worry, I'll automatically take care of everything!");
                            cur_content = targetContents.GetNewContent(contentDetails);
                            cur_content_size = 0;
                        }
                        cur_content_size += child.GetFileSize();

                        if (!child.notInPackage) Console.WriteLine("Set content to " + cur_content.ID .ToString("X") + " ("+cur_content_size.ToString("X")+","+child.GetFileSize().ToString("X") + ") for: " + filePath);
                        child.SetContent(cur_content);
                        result = true;
                    }
                }
            }

            if (result)
                cur_entry.SetContent(cur_content_first);

            return result;
        }
    }
}
