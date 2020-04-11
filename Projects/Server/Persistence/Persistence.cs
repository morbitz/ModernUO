#region References

using System;
using System.IO;

#endregion

namespace Server
{
  public static class Persistence
  {
    public static void Serialize(string path, Action<IGenericWriter> serializer)
    {
      Serialize(new FileInfo(path), serializer);
    }

    public static void Serialize(FileInfo file, Action<IGenericWriter> serializer)
    {
      file.Refresh();

      if (file.Directory?.Exists == false)
        file.Directory.Create();

      if (!file.Exists) file.Create().Close();

      file.Refresh();

      using FileStream fs = file.OpenWrite();
      BinaryFileWriter writer = new BinaryFileWriter(fs, true);

      try
      {
        serializer(writer);
      }
      finally
      {
        writer.Flush();
        writer.Close();
      }
    }

    public static void Deserialize(string path, Action<IGenericReader> deserializer)
    {
      Deserialize(path, deserializer, true);
    }

    public static void Deserialize(FileInfo file, Action<IGenericReader> deserializer)
    {
      Deserialize(file, deserializer, true);
    }

    public static void Deserialize(string path, Action<IGenericReader> deserializer, bool ensure)
    {
      Deserialize(new FileInfo(path), deserializer, ensure);
    }

    public static void Deserialize(FileInfo file, Action<IGenericReader> deserializer, bool ensure)
    {
      file.Refresh();

      if (file.Directory?.Exists == false)
      {
        if (!ensure)
          throw new DirectoryNotFoundException();

        file.Directory.Create();
      }

      if (!file.Exists)
      {
        if (!ensure)
          throw new FileNotFoundException
          {
            Source = file.FullName
          };

        file.Create().Close();
      }

      file.Refresh();

      using FileStream fs = file.OpenRead();
      BinaryFileReader reader = new BinaryFileReader(new BinaryReader(fs));

      try
      {
        deserializer(reader);
      }
      catch (EndOfStreamException eos)
      {
        if (file.Length > 0) Console.WriteLine("[Persistence]: {0}", eos);
      }
      catch (Exception e)
      {
        Console.WriteLine("[Persistence]: {0}", e);
      }
      finally
      {
        reader.Close();
      }
    }
  }
}