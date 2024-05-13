using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using StardewModdingAPI;

namespace StardewMouseTweaks.Extensions;

public static class MonitorExtensions
{
    class CodeInstructionFormatter(int instructionCount)
    {
        private int _instructionIndexPadLength = instructionCount.ToString().Length;

        public string Format(CodeInstruction instruction, int index)
            => $"    IL_{index.ToString().PadLeft(_instructionIndexPadLength, '0')}: {instruction}";
    }

    public static void LogInstructionsFrom(this IMonitor monitor, CodeMatcher matcher, LogLevel level)
    {
        var methodName = new StackTrace().GetFrame(1)?.GetMethod()?.Name ?? "Unknown";

        var instructionFormatter = new CodeInstructionFormatter(matcher.Length);
        var builder = new StringBuilder($"'{methodName}' Matcher Instructions:\n")
            .AppendLine(
                String.Join(
                    "\n",
                    matcher
                        .InstructionEnumeration()
                        .Select(instructionFormatter.Format)
                )
            )
            .AppendLine("End of matcher instructions.");

        monitor.Log(builder.ToString(), level);
    }
}
