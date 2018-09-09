using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MHArmory.ScalableVectorGraphics
{
    public enum PathCommand
    {
        Unset,
        MoveTo,
        LineTo,
        HorizontalLineTo,
        VerticalLineTo,
        CubicBezier,
        Close
    }

    public struct PathElement
    {
        public PathCommand Command { get; }
        public bool IsAbsolute { get; }
        public double Value { get; }

        public PathElement(PathCommand command, bool isAbsolute, double value)
        {
            Command = command;
            IsAbsolute = isAbsolute;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Command} {Value} [{(IsAbsolute ? "Absolute" : "Relative")}]";
        }
    }

    public static class PathLexer
    {
        public static IList<PathElement> Process(string pathDataString)
        {
            if (pathDataString == null)
                throw new ArgumentNullException(nameof(pathDataString));

            var result = new List<PathElement>();

            if (pathDataString == "none")
                return result;

            // As long as double.TryParse does not take indices to parse at a given location,
            // it is pointless to try to optimize code by not using the crazy expensive [string].Split method.
            string[] parts = pathDataString.Split(' ');

            bool isAbsolute = true;

            for (int i = 0; i < parts.Length; i++)
            {
                double numericValue;

                string stringValue = parts[i];

                char startSymbol = stringValue[0];
                char endSymbol = stringValue[stringValue.Length - 1];

                bool isStartSymbol = char.IsLetter(startSymbol);
                bool isEndSymbol = char.IsLetter(endSymbol);

                PathCommand pathCommand = PathCommand.Unset;

                if (isStartSymbol && isEndSymbol)
                    throw new FormatException($"Invalid path element containing two symbols '{stringValue}'.");

                if (isStartSymbol)
                {
                    if (double.TryParse(stringValue.Substring(1), out numericValue) == false)
                        throw new FormatException($"Invalid coordintate value '{stringValue}'.");

                    isAbsolute = char.IsUpper(startSymbol);

                    if (isAbsolute == false && char.IsLetter(startSymbol) == false)
                        throw new FormatException($"Unknown command symbol '{stringValue}'.");

                    pathCommand = SymbolToCommand(startSymbol);
                }
                else if (isEndSymbol)
                {
                    if (endSymbol != 'z' && endSymbol != 'Z')
                        throw new FormatException($"Invalid closing symbol '{stringValue}'.");

                    if (double.TryParse(stringValue.Substring(0, stringValue.Length - 1), out numericValue) == false)
                        throw new FormatException($"Invalid coordintate value '{stringValue}'.");
                }
                else
                {
                    if (double.TryParse(stringValue, out numericValue) == false)
                        throw new FormatException($"Invalid coordintate value '{stringValue}'.");
                }

                if (i == 0 && pathCommand == PathCommand.Unset)
                    throw new FormatException("Path not properly opened.");

                result.Add(new PathElement(pathCommand, isAbsolute, numericValue));

                if (isEndSymbol)
                    result.Add(new PathElement(PathCommand.Close, char.IsUpper(endSymbol), 0.0));
            }

            return result;
        }

        private static PathCommand SymbolToCommand(char symbol)
        {
            switch (symbol)
            {
                case 'M':
                case 'm':
                    return PathCommand.MoveTo;
                case 'L':
                case 'l':
                    return PathCommand.LineTo;
                case 'H':
                case 'h':
                    return PathCommand.HorizontalLineTo;
                case 'V':
                case 'v':
                    return PathCommand.VerticalLineTo;
                case 'C':
                case 'c':
                    return PathCommand.CubicBezier;
            }

            throw new NotSupportedException($"Path command '{symbol}' is not supported.");
        }
    }
}
