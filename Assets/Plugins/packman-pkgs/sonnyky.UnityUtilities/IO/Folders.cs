using System.IO;

public static class Folders
{
    public static DirectoryInfo Create(string path)
    {
        DirectoryInfo folder = Directory.CreateDirectory(path);
        return folder;
    }

}
