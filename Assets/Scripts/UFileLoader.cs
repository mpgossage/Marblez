using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Set of routines to work around Unity's non support for System.File in browser
public static class UFileLoader 
{
    /** Loads all lines from a textfile (similar to File.ReadLines)
     */
    public static string[] ReadLines(string filename)
    {
        // instead of using File.Read() we need to use the unity resources instead.
        // create a directory called 'Resources' & put them in there
        TextAsset asset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        return asset.text.Split('\n');
    }
    /** Loads all lines from a textfile in a single string (similar to File.ReadAllText)
     */
    public static string ReadAllText(string filename)
    {
        // instead of using File.Read() we need to use the unity resources instead.
        // create a directory called 'Resources' & put them in there
        TextAsset asset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        return asset.text;
    }

    /** Loads a text file, ignoring lines which begin with a non alpha numeric
     */
    public static List<string> LoadScriptFile(string filename)
    {
        List<string> result = new List<string>();
        foreach (string line in ReadLines(filename))
        {
            // only add if not empty & starts with letter/digit
            if (line.Length > 0 && char.IsLetterOrDigit(line, 0))
                result.Add(line);
        }
        return result;
    }

    /** Loads a text file as a CSV (comma seperated variables)
     * (also quietly ignores comment & empty lines)
     */
    public static List<string[]> LoadCsvFile(string filename)
    {
        List<string[]> result = new List<string[]>();
        foreach (string line in ReadLines(filename))
        {
            // only add if not empty & starts with letter/digit
            if (line.Length > 0 && char.IsLetterOrDigit(line, 0))
                result.Add(line.Split(','));
        }
        return result;
    }

}
