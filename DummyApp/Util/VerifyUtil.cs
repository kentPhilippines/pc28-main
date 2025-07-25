using System.Windows.Forms;

namespace DummyApp.Util;

internal class VerifyUtil
{
    /// <summary>
    ///     尝试获取空的文本输入框式，验证多个文本框不为空时使用
    /// </summary>
    /// <param name="empty"></param>
    /// <param name="textBoxes"></param>
    /// <returns></returns>
    public static bool TryGetEmptyTextBox(out TextBox empty, params TextBox[] textBoxes)
    {
        foreach (TextBox textBox in textBoxes)
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                empty = textBox;
                return true;
            }

        empty = null;
        return false;
    }
}