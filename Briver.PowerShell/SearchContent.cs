using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Briver.PowerShell
{
    [Alias("grep")]
    [Cmdlet(VerbsCommon.Search, "Content")]
    public class SearchContent : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PSObject Input { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Pattern { get; set; }

        [Parameter]
        public SwitchParameter UseRegex { get; set; } = false;

        [Parameter]
        public SwitchParameter CaseSensitive { get; set; } = false;

        protected override void ProcessRecord()
        {
            if (this.Input == null) return;
            if (String.IsNullOrEmpty(this.Pattern))
            {
                this.WriteObject(this.Input);
            }

            var pattern = this.Pattern;
            if (!this.UseRegex)
            {
                pattern = pattern.Replace("*", ".*").Replace("?", ".?");
            }
            var options = RegexOptions.Singleline | RegexOptions.ExplicitCapture;
            if (!this.CaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }
            var regex = new Regex(pattern, options);

            if (this.Input is IEnumerable collection)
            {
                foreach (var item in collection)
                {
                    if (IsMatch(regex, item))
                    {
                        this.WriteObject(item);
                        continue;
                    }
                }
            }
            else if (IsMatch(regex, this.Input))
            {
                this.WriteObject(this.Input);
            }
        }

        private bool IsMatch(Regex regex, object content)
        {
            if (content == null) { return false; }
            if (content is IFormattable format)
            {
                if (regex.IsMatch(format.ToString(null, CultureInfo.CurrentCulture) ?? String.Empty))
                {
                    return true;
                }
            }
            else
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(content))
                {
                    if (IsMatch(regex, property.GetValue(content)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}