namespace LearnOpenGL.OpenTK.Core;

public sealed record RunOptions(
    string DemoId,
    int Width,
    int Height,
    int Frames,
    string? CapturePath,
    bool List,
    string? ModelStatsPath)
{
    public static RunOptions Parse(string[] args)
    {
        var demoId = "";
        var width = 800;
        var height = 600;
        var frames = 60;
        string? capturePath = null;
        string? modelStatsPath = null;
        var list = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--list":
                    list = true;
                    break;
                case "--width":
                    width = int.Parse(RequireValue(args, ref i));
                    break;
                case "--height":
                    height = int.Parse(RequireValue(args, ref i));
                    break;
                case "--frames":
                    frames = int.Parse(RequireValue(args, ref i));
                    break;
                case "--capture":
                    capturePath = RequireValue(args, ref i);
                    break;
                case "--model-stats":
                    modelStatsPath = RequireValue(args, ref i);
                    break;
                default:
                    if (demoId.Length == 0)
                    {
                        demoId = args[i];
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected argument: {args[i]}");
                    }

                    break;
            }
        }

        return new RunOptions(demoId, width, height, frames, capturePath, list, modelStatsPath);
    }

    private static string RequireValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {args[index]}");
        }

        index++;
        return args[index];
    }
}
