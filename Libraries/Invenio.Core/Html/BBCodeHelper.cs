using System;
using System.Text.RegularExpressions;
using Invenio.Core.Domain.Common;
using Invenio.Core.Html.CodeFormatter;
using Invenio.Core.Infrastructure;

namespace Invenio.Core.Html
{
    /// <summary>
    /// Represents a BBCode helper
    /// </summary>
    public partial class BBCodeHelper
    {
        #region Fields
        private static readonly Regex regexBold = new Regex(@"\[b\](.+?)\[/b\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexItalic = new Regex(@"\[i\](.+?)\[/i\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexUnderLine = new Regex(@"\[u\](.+?)\[/u\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexUrl1 = new Regex(@"\[url\=([^\]]+)\]([^\]]+)\[/url\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexUrl2 = new Regex(@"\[url\](.+?)\[/url\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexQuote = new Regex(@"\[quote=(.+?)\](.+?)\[/quote\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regexImg = new Regex(@"\[img\](.+?)\[/img\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion

        #region Methods
        /// <summary>
        /// Formats the text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="replaceBold">A value indicating whether to replace Bold</param>
        /// <param name="replaceItalic">A value indicating whether to replace Italic</param>
        /// <param name="replaceUnderline">A value indicating whether to replace Underline</param>
        /// <param name="replaceUrl">A value indicating whether to replace URL</param>
        /// <param name="replaceCode">A value indicating whether to replace Code</param>
        /// <param name="replaceQuote">A value indicating whether to replace Quote</param>
        /// <param name="replaceImg">A value indicating whether to replace Img</param>
        /// <returns>Formatted text</returns>
        public static string FormatText(string text, bool replaceBold, bool replaceItalic,
            bool replaceUnderline, bool replaceUrl, bool replaceCode, bool replaceQuote, bool replaceImg)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            if (replaceBold)
            {
                // format the bold tags: [b][/b]
                // becomes: <strong></strong>
                text = regexBold.Replace(text, "<strong>$1</strong>");
            }

            if (replaceItalic)
            {
                // format the italic tags: [i][/i]
                // becomes: <em></em>
                text = regexItalic.Replace(text, "<em>$1</em>");
            }

            if (replaceUnderline)
            {
                // format the underline tags: [u][/u]
                // becomes: <u></u>
                text = regexUnderLine.Replace(text, "<u>$1</u>");
            }

            if (replaceUrl)
            {
                var newWindow = EngineContext.Current.Resolve<CommonSettings>().BbcodeEditorOpenLinksInNewWindow;
                text = regexUrl1.Replace(text, string.Format("<a href=\"$1\" rel=\"nofollow\"{0}>$2</a>", newWindow ? " target=_blank" : ""));

                text = regexUrl2.Replace(text, string.Format("<a href=\"$1\" rel=\"nofollow\"{0}>$1</a>", newWindow ? " target=_blank" : ""));
            }

            if (replaceQuote)
            {
                while (regexQuote.IsMatch(text))
                    text = regexQuote.Replace(text, "<b>$1 wrote:</b><div class=\"quote\">$2</div>");
            }

            if (replaceCode)
            {
                text = CodeFormatHelper.FormatTextSimple(text);
            }

            if (replaceImg)
            {
                text = regexImg.Replace(text, "<img src=\"$1\" class=\"user-posted-image\" alt=\"\">");
            }
            return text;
        }

        /// <summary>
        /// Removes all quotes from string
        /// </summary>
        /// <param name="str">Source string</param>
        /// <returns>string</returns>
        public static string RemoveQuotes(string str)
        {
            str = Regex.Replace(str, @"\[quote=(.+?)\]", String.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\[/quote\]", String.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return str;
        }

        #endregion
    }
}
