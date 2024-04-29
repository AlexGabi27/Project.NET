
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

internal class ConsoleApp2
{
    static int linesNumber;

    private static bool IsNumber(char input)
    {
        if (input >= '0' && input <= '9')
        {
            return true;
        }

        return false;
    }

    private static int GetColNumber(string[] input)
    {
        int colNum = 0;

        foreach (string s in input)
        {
            int i = 0;
            for (i = 0; i < s.Length; i++)
            {
                if (IsNumber(s[i]) == true)
                {
                    break;  // looking for a position in string where we have a number and save the index into i
                }
            }

            if (i < s.Length)   // checking if we are not at the end of the string => means we can do further checking
            {
                if (IsNumber(s[i]) == true) // chck if the position is a number
                {
                    for (int j = 0; j < s.Length; j++)  // and now loop through the string to see if it is what we need
                    {
                        if (IsNumber(s[j]) == true) // check again if the position is a number
                        {
                            if (j == s.Length - 1)  // check if we are at the end of the string
                            {
                                if (IsNumber(s[j - 1]) == true) // check if before this number, we have another number => a bigger column number
                                {
                                    // it means we have a bigger column number and we have to get the full number
                                    //Console.WriteLine("We have a bigger number");
                                    int x = j;
                                    for (; s[j] != ' '; j--) ;

                                    j++;

                                    for (; j < s.Length; j++)
                                    {
                                        colNum += s[j] - '0';
                                        colNum *= 10;
                                    }
                                    colNum /= 10;
                                }
                                else
                                {
                                    colNum = s[j] - '0';
                                }
                            }
                        }
                        else
                        {
                            if (s[j] != ' ')
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        return colNum;
    }

    private static bool CheckPosition(string input)
    {
        if (input[0] == '[' && input[input.Length - 1] == ']')
        {
            return true;
        }
        return false;
    }

    private static bool CheckIfWhiteSpace(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] != ' ')
            {
                return false;
            }
        }

        return true;
    }

    private static List<int> ProcessInputData(string input, int colNum)
    {
        List<int> positions = new List<int>();

        int linesCount = 0;
        int crateCount = 0;

        for (int i = 0; i < input.Length;)
        {
            if (linesCount >= linesNumber)
            {
                break;
            }
            if (i + 3 < input.Length)
            {
                if (input[i] == '[' || input[i] == ' ')
                {
                    string s = input.Substring(i, 3);
                    if (CheckPosition(s) == true)
                    {
                        positions.Add(i);
                        positions.Add(crateCount++);

                        i += 3;
                    }
                    else if (CheckIfWhiteSpace(s) == true)
                    {
                        positions.Add(i);
                        positions.Add(crateCount++);

                        i += 4;
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (input[i] == '\r' || input[i] == '\n')
                {
                    if (input[i] == '\r')
                        i += 2;
                    else
                        i += 1;
                    linesCount++;
                    crateCount = 0;
                }
            }
        }

        return positions;
    }

    private static List<int> GetMovesFromInput(string[] input)
    {
        List<string> movesList = new List<string>();
        List<int> moves = new List<int>();

        foreach (string s in input)
        {
            if (s.Contains("move"))
            {
                movesList.Add(s);
            }
        }

        foreach (string s in movesList)  // generate pairs of 3 in the List -> number of crates | source | destination
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] >= '0' && s[i] <= '9')
                {
                    int num = 0;
                    while (s[i] != ' ')
                    {
                        num += s[i++] - '0';
                        num *= 10;

                        if (i >= s.Length)  // check if this is the last number from the moves line
                        {
                            break;
                        }
                    }

                    num /= 10;
                    moves.Add(num);
                }
            }
        }

        return moves;
    }

    private static int[] GetCrateCoordinates(List<int> data, int columnsNumber, int src, char[] input)
    {
        int[] coords = new int[2];

        int j = 0;

        for (j = 0; j < data.Count; j += 2)
        {
            if (data[j + 1] == src - 1)
            {
                if (input[data[j]] == ' ')
                {
                    coords[0] = data[j];
                    coords[1] = data[j + 1];
                }
                else if (input[data[j]] == '[')
                {
                    coords[0] = data[j];
                    coords[1] = data[j + 1];
                    break;
                }
            }
        }

        return coords;
    }

    private static bool CountSpaces(char[] input, int pos)
    {
        if (pos == 0)
            return false;

        int spaces = 0;
        for (int i = pos - 1; i > 0; i--)
        {
            if (input[i] == '\r' || input[i] == '\n')
            {
                break;
            }

            if (input[i] == ']' && spaces == 0)
            {
                return true;
            }
            else
            {
                spaces++;
            }
        }

        if (pos % 2 == 0 && input[pos - 1] != '\n')
        {
            spaces++;
        }

        while (spaces > 0)
        {
            spaces -= 4;
        }

        if (spaces != 0)
        {
            return true;
        }

        return false;
    }

    private static int[] GetEmptySpot(List<int> data, int columnsNumber, int dst, char[] input)
    {
        int[] coords = new int[2];

        int j = 0;

        for (j = data.Count - 2; j >= 0; j -= 2)
        {
            if (data[j + 1] == dst - 1)
            {
                if (input[data[j]] == ' ')
                {
                    if (CountSpaces(input, data[j]) == true /*input[data[j] - 1] == ']' || (input[data[j] - 1] == ' ' && input[data[j] - 2] == ' ')*/)
                    {
                        data[j]++;
                    }
                    coords[0] = data[j];
                    coords[1] = data[j + 1];
                    break;
                }
            }
        }

        return coords;
    }

    private static int ColumnItemNumbers(List<int> data, int dst, char[] input)
    {
        int itemNumber = 0;

        for (int i = 1; i < data.Count; i++)
        {
            if (data[i] == dst - 1 && input[data[i - 1]] == '[')
            {
                itemNumber++;
            }
        }

        return itemNumber;
    }

    private static char[] MakeAMove(char[] input, int src, int dst, List<int> data, int columnsNumber)
    {
        int itemNumbers = ColumnItemNumbers(data, dst, input);

        if (itemNumbers >= linesNumber)
        {
            linesNumber++;

            int len = 3 * columnsNumber + columnsNumber - 1;
            input = AddWhiteSpace(new string(input), 0, len);

            string t = new string(input);
            input = t.Insert(len, "\r\n").ToCharArray();

            t = new string(input);

            data = ProcessInputData(new string(input), columnsNumber);
        }

        int[] crate_location_src = GetCrateCoordinates(data, columnsNumber, src, input);
        int[] crate_location_dst = GetCrateCoordinates(data, columnsNumber, dst, input);

        int[] empty_spot_dst = GetEmptySpot(data, columnsNumber, dst, input);

        if (input[empty_spot_dst[0]] == ' ')
        {
            input[empty_spot_dst[0]] = '[';
            input[empty_spot_dst[0] + 1] = input[crate_location_src[0] + 1];
            input[empty_spot_dst[0] + 2] = ']';

            input[crate_location_src[0]] = ' ';
            input[crate_location_src[0] + 1] = ' ';
            input[crate_location_src[0] + 2] = ' ';
        }

        return input;
    }

    private static bool IsLineBlank(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] != ' ' || line[i] != '\r' || line[i] != '\n')
            {
                return false;
            }
        }

        return true;
    }

    private static int GetLinesNumber(string[] input)
    {
        int lines = 0;

        foreach (string line in input)
        {
            if (line.Contains("[") || IsLineBlank(line))
            {
                lines++;
            }
        }

        return lines;
    }

    private static char[] AddWhiteSpace(string input, int pos, int spaces)
    {
        char[] ax;
        for (int x = 0; x < spaces; x++)
        {
            ax = input.Insert(pos, " ").ToCharArray();
            input = new string(ax);
        }

        return input.ToCharArray();
    }

    private static void Main(string[] args)
    {
        string input_file = "input.txt";

        string[] text_lines = File.ReadAllLines(input_file);

        // process the number of columns
        int columnsNumber = GetColNumber(text_lines);
        linesNumber = GetLinesNumber(text_lines) - 1;
        List<int> moves = GetMovesFromInput(text_lines);

        // read all file into the text variable
        string text = File.ReadAllText(input_file);

        int crateCount = 0;
        int linesCount = 0;
        int threeWhiteSpaces = 0;

        for (int i = 0; i < text.Length;)
        {
            if (linesCount >= linesNumber)
            {
                break;
            }
            if (i + 3 < text.Length)
            {
                if (text[i] == '[' || text[i] == ' ')
                {
                    if (CheckPosition(text.Substring(i, 3)) == true)
                    {
                        crateCount++;
                        i += 3;
                    }
                    else if (CheckIfWhiteSpace(text.Substring(i, 3)) == true)
                    {
                        if (i != 0)
                        {
                            if (text[i - 1] == '[' || threeWhiteSpaces > 0)
                            {
                                threeWhiteSpaces = 0;
                                i++;
                            }
                            else
                            {
                                crateCount++;
                                threeWhiteSpaces++;
                                i += 3;
                            }
                        }
                        else
                        {
                            crateCount++;
                            threeWhiteSpaces++;
                            i += 3;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (text[i] == '\r')
                {
                    if (crateCount < columnsNumber)
                    {
                        for (int j = 0; j < columnsNumber - crateCount; j++)
                        {
                            int spaces = 4;
                            //if (j == columnsNumber - crateCount - 1)  // possible issue, sometimes some positions need to be shifted with 3 characters, rather than 4
                            //{
                            //    spaces = 3;
                            //}
                            char[] ax = AddWhiteSpace(text, i, spaces);
                            text = new string(ax);
                            i += spaces;
                        }
                        i++;
                        crateCount = 0;
                        linesCount++;
                    }
                    else
                    {
                        i += 2;
                        linesCount++;
                        crateCount = 0;
                    }
                }
                else if (text[i] == '\n')
                {
                    i++;
                }
            }
        }

        // we doing a magic trick with the input data
        List<int> big_data = ProcessInputData(text, columnsNumber);

        char[] text_char = text.ToCharArray();

        for (int i = 0; i < moves.Count; i += 3)
        {
            for (int j = 0; j < moves[i]; j++)
            {
                text_char = MakeAMove(text_char, moves[i + 1], moves[i + 2], big_data, columnsNumber);
                Console.WriteLine(text_char);
                big_data = ProcessInputData(new string(text_char), columnsNumber);
            }
        }

        Console.Write("The message for the Elves: ");

        for (int i = 0; i < columnsNumber; i++)
        {
            for (int j = 0; j < big_data.Count; j += 2)
            {
                if (big_data[j + 1] == i)
                {
                    if (text_char[big_data[j]] == '[')
                    {
                        Console.Write(text_char[big_data[j] + 1]);
                        break;
                    }
                }
            }
        }
        Console.WriteLine("");
    }
}