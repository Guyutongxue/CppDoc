// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Net.Http.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{
    public sealed partial class EditorWebView : UserControl
    {
        public EditorWebView()
        {
            this.InitializeComponent();
            var _ = LoadPage("");
        }
        public EditorWebView(string initCode)
        {
            this.InitializeComponent();
            var _ = LoadPage(initCode);
        }


        public async Task LoadPage(string initCode)
        {
            await webview.EnsureCoreWebView2Async();
            webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets", "Assets", CoreWebView2HostResourceAccessKind.Allow);

            webview.Source = new Uri($"http://appassets/www/index.html?code={Uri.EscapeDataString(initCode)}");
            // webview.CoreWebView2.OpenDevToolsWindow();
        }

        public async Task SetCode(string v)
        {
            await webview.EnsureCoreWebView2Async();
            var json = JsonValue.Create(v);
            System.Diagnostics.Debug.WriteLine(json?.ToJsonString() ?? "NULL");
            if (json is null)
            {
                return; // TODO
            }
            var jsonStr = json.ToJsonString();
            await webview.CoreWebView2.ExecuteScriptAsync($"setCode({jsonStr})");
        }

        record class Stdio(
            string text);

        record class ExecResult(
            int code,
            bool didExecute,
            List<Stdio>? stdout,
            List<Stdio>? stderr);

        record class GodboltResponse(
            ExecResult execResult);

        private async void Compile(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonCompile.IsEnabled = false;
                textBlockOut.Text = "获取代码中";
                await webview.EnsureCoreWebView2Async();
                var result = await webview.ExecuteScriptAsync("getCode()");
                if (result is null)
                {
                    textBlockOut.Text = "获取出错";
                    return;
                }
                var code = JsonDocument.Parse(result).RootElement.GetString();
                if (result is null)
                {
                    textBlockOut.Text = "解析出错";
                    return;
                }
                var client = new HttpClient();
                var payload = JsonContent.Create(new
                {
                    source = code,
                    options = new
                    {
                        userArguments = "-std=c++2b -Wall -Wextra -pedantic-errors",
                        executeParameters = new { },
                        filters = new
                        {
                            execute = true,
                        },
                    },
                    lang = "cpp",
                });
                textBlockOut.Text = "发送编译请求中";
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://godbolt.org/api/compiler/g122/compile");
                request.Content = payload;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                textBlockOut.Text = "发送编译请求中";
                using var response = await client.SendAsync(request);
                var responseJson = await response.Content.ReadFromJsonAsync<GodboltResponse>();
                if (responseJson?.execResult?.didExecute ?? false)
                {
                    textBlockOut.Text = string.Join(Environment.NewLine, responseJson!.execResult.stdout!.Select(s => s.text));
                }
                else
                {
                    textBlockOut.Text = "出错了";
                }
            }
            finally
            {
                buttonCompile.IsEnabled = true;
            }
        }
    }
}
