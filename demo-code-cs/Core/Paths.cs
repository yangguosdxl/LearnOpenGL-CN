namespace LearnOpenGL.OpenTK.Core;

public static class Paths
{
    public static string Resource(string relativePath)
    {
        return Existing(Path.Combine(AppContext.BaseDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    public static string Shader(string chapter, string demo, string fileName)
    {
        return Existing(Path.Combine(AppContext.BaseDirectory, "src", chapter, demo, fileName));
    }

    private static string Existing(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Required file was not copied to output: {path}", path);
        }

        return path;
    }
}
