using CNUS_packer.contents;
using CNUS_packer.fst;

using System;
using System.Text.RegularExpressions;

namespace CNUS_packer.packaging
{
    public class ContentRulesService
    {
        public static long MAX_CONTENT_LENGTH = (long)(0xBFFFFFFFL * 0.975);

        public static long cur_content_size = 0L;

        public static Content cur_content = null;
        public static Content cur_content_first = null;

        public static void applyRules(FSTEntry root, Contents targetContents, ContentRules rules)
        {
            Console.WriteLine("-----");
            foreach (ContentRules.ContentRule rule in rules.GetRules())
            {
                Console.WriteLine("Apply rule \"" + rule.getPattern() + "\"");
                if (rule.isContentPerMatch())
                {
                    setNewContentRecursiveRule("", rule.getPattern(), root, targetContents, rule);
                }
                else
                {
                    cur_content = targetContents.getNewContent(rule.getDetails());
                    cur_content_first = cur_content;
                    cur_content_size = 0L;
                    bool result = setContentRecursiveRule("", rule.getPattern(), root, targetContents, rule.getDetails());
                    if (!result)
                    {
                        Console.WriteLine("No file matched the rule. Lets delete the content again");
                        targetContents.deleteContent(cur_content);
                    }
                    cur_content_first = null;
                }
                Console.WriteLine("-----");
            }
        }

        private static Content setNewContentRecursiveRule(string path, string pattern, FSTEntry cur_entry, Contents targetContents, ContentRules.ContentRule rule)
        {
            path += cur_entry.getFilename() + "/";
            Regex p =  new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Content result = null;

            if (cur_entry.getChildren().Count == 0)
            {
                MatchCollection m = p.Matches(path);
                // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                if (m.Count > 0)
                {
                    Content result_content = targetContents.getNewContent(rule.getDetails());
                    result = result_content;
                }
            }
            foreach (FSTEntry child in cur_entry.getChildren())
            {
                if (child.getIsDir())
                {
                    Content child_result = setNewContentRecursiveRule(path, pattern, child, targetContents, rule);
                    if (child_result != null)
                    {
                        result = child_result;
                    }
                }
                else
                {
                    string filePath = path + child.getFilename();
                    Match m = p.Match(filePath);
                    // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                    if (m.Success)
                    {
                        Content result_content = targetContents.getNewContent(rule.getDetails());
                        if (!child.isNotInPackage()) Console.WriteLine("Set content to " + result_content.ID.ToString("X") + " for: " + filePath);
                        child.setContent(result_content);
                        result = result_content;
                    }
                }
            }
            if (result != null)
            {
                cur_entry.setContent(result);
            }
            return result;
        }

        private static bool setContentRecursiveRule(string path, string pattern, FSTEntry cur_entry, Contents targetContents, ContentDetails contentDetails)
        {
            path += cur_entry.getFilename() + "/";
            Regex p = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool result = false;
            if (cur_entry.getChildren().Count == 0)
            {
                Match m = p.Match(path);
                // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                if (m.Success)
                {
                    if (!cur_entry.isNotInPackage()) Console.WriteLine("Set content to " + cur_content.ID.ToString("X") + " (" + cur_content_size.ToString("X") + "," + cur_entry.getFilesize().ToString("X") + ") for: " + path);
                    if (cur_entry.getChildren().Count == 0/* && cur_entry.getFilename().equals("content")*/)
                    {  //TODO: may could cause problems. Current solution only apply to content folder.
                        cur_entry.setContent(cur_content);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            foreach (FSTEntry child in cur_entry.getChildren())
            {
                if (child.getIsDir())
                {

                    bool child_result = setContentRecursiveRule(path, pattern, child, targetContents, contentDetails);
                    if (child_result)
                    {
                        cur_entry.setContent(cur_content_first);
                        result = true;
                    }
                }
                else
                {
                    string filePath = path + child.getFilename();
                    Match m = p.Match(filePath);
                    // Console.WriteLine("Trying rule \"" + p + "\" for string \"" + filePath + "\"");
                    if (m.Success)
                    {
                        if (cur_content_size > 0 && (cur_content_size + child.getFilesize()) > MAX_CONTENT_LENGTH)
                        {
                            Console.WriteLine("Info: Target content size is bigger than " + MAX_CONTENT_LENGTH + " bytes. Content will be splitted in mutitple files. Don't worry, I'll automatically take care of everything!");
                            cur_content = targetContents.getNewContent(contentDetails);
                            cur_content_size = 0;
                        }
                        cur_content_size += child.getFilesize();

                        if (!child.isNotInPackage()) Console.WriteLine("Set content to " + cur_content.ID .ToString("X") + " ("+cur_content_size.ToString("X")+","+child.getFilesize().ToString("X") + ") for: " + filePath);
                        child.setContent(cur_content);
                        result = true;
                    }
                }
            }

            if (result)
            {
                cur_entry.setContent(cur_content_first);
            }

            return result;
        }
    }
}
