using System.Windows.Forms;

namespace SaigonBite.UI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var ctx = new DemoAppContext();
        Application.Run(new SaigonBiteShellForm(ctx));
    }
}
