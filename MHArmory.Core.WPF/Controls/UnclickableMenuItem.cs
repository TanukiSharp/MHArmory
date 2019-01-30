using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MHArmory.Core.WPF.Controls
{
    public class UnclickableMenuItem : MenuItem
    {
        public UnclickableMenuItem()
        {
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            //base.OnMouseEnter(e);
        }

        protected override void OnClick()
        {
            //base.OnClick();
        }
    }
}
