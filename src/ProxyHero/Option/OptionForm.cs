﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Loamen.Common;
using Loamen.Net.Entity;
using Loamen.WinControls.UI;
using Loamen.WinControls.UI.Collections;
using ProxyHero.Common;
using ProxyHero.Entity;
using ProxyHero.Option.Panels;

namespace ProxyHero.Option
{
    public partial class OptionForm : OptionsForm
    {
        public OptionForm()
            : base(PropertyDictionary<string, object>.Convert(Config.LocalSetting))
        {
            InitializeComponent();

            Panels.Add(new GeneralPanel());
            Panels.Add(new TestPanel());
            Panels.Add(new LanguagePanel());
            Panels.Add(new UserAgentPanel());
            Panels.Add(new SystemTestPanel());

            AppRestartText = Config.LocalLanguage.OptionPage.ProgramRestartRequired;

            Config.LocalSetting.PropertyChanged += LocalSetting_PropertyChanged;
        }

        private void LocalSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _AppSettings_SettingChanging(sender, e);
        }

        public override void OnSaveOptions()
        {
            base.OnSaveOptions();
            try
            {
                Config.LocalSetting.PropertyChanged -= LocalSetting_PropertyChanged;
                Save();
            }
            catch (Exception ex)
            {
                MsgBox.ShowExceptionMessage(ex);
            }
        }

        private void Save()
        {
            try
            {
                SaveLoginSetting();
            }
            catch{}
        }

        /// <summary>
        ///     保存登录信息
        /// </summary>
        private void SaveLoginSetting()
        {
            var localSetting = Config.LocalSetting;
            var testSetting = (TestOption)AppSettings["DefaultTestOption"];
            TestOption searchSetting = FindLoginSettingByAccount(localSetting, testSetting.TestUrl);


            localSetting.DefaultTestOption = testSetting;
            localSetting.ScriptErrorsSuppressed = (bool)AppSettings["ScriptErrorsSuppressed"];
            localSetting.AutoChangeProxyInterval = (int)AppSettings["AutoChangeProxyInterval"];
            localSetting.TestTimeOut = (int)AppSettings["TestTimeOut"];
            localSetting.TestThreadsCount = (int)AppSettings["TestThreadsCount"];
            localSetting.AutoProxySpeed = (int)AppSettings["AutoProxySpeed"];
            localSetting.CheckArea = (bool)AppSettings["CheckArea"];
            localSetting.ExportMode = (string)AppSettings["ExportMode"];
            localSetting.IsUseSystemProxy = (bool)AppSettings["IsUseSystemProxy"];
            localSetting.LanguageFileName = (string)AppSettings["LanguageFileName"];
            localSetting.UserAgent = string.IsNullOrEmpty((string)AppSettings["UserAgent"])
                                                 ? "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322)"
                                                 : (string)AppSettings["UserAgent"];

            if (searchSetting != null && searchSetting.TestUrl != "")
            {
                if (searchSetting.TestUrl == testSetting.TestUrl) //如果已经设置，则移除换成新的
                {
                    localSetting.TestOptionsList.Remove(searchSetting);
                    localSetting.DefaultTestOption = testSetting;
                }
            }

            if (!Contains(localSetting,testSetting))
            {
                localSetting.TestOptionsList.Add(testSetting);
            }

            var res = XmlHelper.XmlSerialize(Config.SettingFileName, localSetting, typeof(Setting));
            if (!res)
            {
                MsgBox.ShowErrorMessage("保存失败！");
            }
        }

        /// <summary>
        ///     通过帐号获取登录配置信息
        /// </summary>
        /// <param name="localSetting"></param>
        /// <param name="testUrl"></param>
        /// <returns></returns>
        public TestOption FindLoginSettingByAccount(Setting localSetting,string testUrl)
        {
            return localSetting.TestOptionsList.FirstOrDefault(temp => temp.TestUrl == testUrl);
        }

        private bool Contains(Setting localSetting, TestOption testEntity)
        {
            return localSetting.TestOptionsList.Any(te => te.TestUrl.ToLower() == testEntity.TestUrl.ToLower());
        }
    }
}
