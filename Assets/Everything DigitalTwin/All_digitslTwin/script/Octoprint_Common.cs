using System.Collections.Generic;

[System.Serializable]
public class FilesResponse
{
    public List<File> files;
}

[System.Serializable]
public class File
{
    public string name;
    public string path;
    public string type;
    public long size;
}