namespace SkyblivionVoicesRemastered;

class Utilities
{
  public static bool CheckHeader(byte[] header, string expected)
  {
    if (header.Length < expected.Length)
      return false;
    for (var i=0; i < expected.Length; i++)
    {
      if (header[i] != expected[i])
        return false;
    }
    return true;
  }
}
