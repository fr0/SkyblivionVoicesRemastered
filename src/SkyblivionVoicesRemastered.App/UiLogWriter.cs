using System.Text;

namespace SkyblivionVoicesRemastered.App;

/// <summary>
/// TextWriter that appends lines to a TextBox from any thread, batching to keep the UI responsive
/// </summary>
public class UiLogWriter : TextWriter
{
  private readonly TextBox _target;
  private readonly StringBuilder _pending = new();
  private readonly StringBuilder _line = new();
  private readonly System.Windows.Forms.Timer _timer;
  private readonly Lock _lock = new();

  const int BatchIntervalMs = 150;

  public UiLogWriter(TextBox target)
  {
    _target = target;
    _timer = new System.Windows.Forms.Timer { Interval = BatchIntervalMs };
    _timer.Tick += (_, _) => Flush();
    _timer.Start();
  }

  public override Encoding Encoding => Encoding.UTF8;

  public override void Write(char value)
  {
    lock (_lock)
    {
      if (value == '\n')
      {
        _pending.Append(_line).Append(Environment.NewLine);
        _line.Clear();
      }
      else if (value != '\r') // skip pesky carriage returns (thanks Windows)
        _line.Append(value);
    }
  }

  public override void Write(string? value)
  {
    if (value == null)
      return;
    foreach (var ch in value)
      Write(ch);
  }

  public override void WriteLine(string? value)
  {
    Write(value);
    Write('\n');
  }

  public override void Flush()
  {
    string text;
    lock (_lock)
    {
      if (_pending.Length == 0)
        return;
      text = _pending.ToString();
      _pending.Clear();
    }
    if (_target.IsDisposed)
      return;
    if (_target.InvokeRequired)
      _target.BeginInvoke(() => Append(text));
    else
      Append(text);
  }

  private void Append(string text)
  {
    if (_target.IsDisposed)
      return;
    // maybe this should be a rolling window, but I'm lazy
    if (_target.TextLength > 2_000_000)
      _target.Clear();
    _target.AppendText(text);
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
      _timer.Dispose();
    base.Dispose(disposing);
  }
}
