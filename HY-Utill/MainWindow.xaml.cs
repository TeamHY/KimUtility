﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace HY_Utill
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string storageUrl = @"https://teamhy.github.io/";
        public static string ModsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Binding of Isaac Afterbirth+ Mods";

        public Brush DefaultProgressBarBrush;

        private delegate void CSafeSetMaximum(Int32 value);
        private delegate void CSafeSetValue(Int32 value);

        private CSafeSetMaximum cssm;
        private CSafeSetValue cssv;

        private WebClient wc;
        private bool setBaseSize;
        private bool nowDownloading;

        public MainWindow()
        {
            cssm = new CSafeSetMaximum(CrossSafeSetMaximumMethod);
            cssv = new CSafeSetValue(CrossSafeSetValueMethod);

            wc = new WebClient();

            InitializeComponent();
        }

        private void VersionInfoUpdate()
        {
            VersionUtility.CheckVersion();

            if (VersionUtility.CurrentVersion != null)
                lblCurrentVersion.Content = VersionUtility.CurrentVersion;
            else
                lblCurrentVersion.Content = "N/A";

            if (VersionUtility.LatestVersion != null)
                lblLatestVersion.Content = VersionUtility.LatestVersion;
            else
                lblLatestVersion.Content = "N/A";
        }

        private void StartFileDownload(String remoteAddress, String localPath)
        {
            if (nowDownloading)
            {
                MessageBox.Show("이미 다운로드가 진행 중입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (String.IsNullOrEmpty(remoteAddress))
            {
                MessageBox.Show("주소가 입력되지 않았습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // 파일이 저장될 위치를 저장한다.
            String fileName = String.Format(@"{0}\{1}", localPath, System.IO.Path.GetFileName(remoteAddress));

            // 폴더가 존재하지 않는다면 폴더를 생성한다.
            if (!System.IO.Directory.Exists(localPath))
                System.IO.Directory.CreateDirectory(localPath);

            try
            {
                // C 드라이브 밑의 downloadFiles 폴더에 파일 이름대로 저장한다.
                wc.DownloadFileAsync(new Uri(remoteAddress), fileName);

                // 다운로드 중이라는걸 알리기 위한 값을 설정하고,
                // 프로그레스바의 크기를 0으로 만든다.
                prgInstall.Value = 0;
                prgInstall.Foreground = Brushes.Gold;
                setBaseSize = false;
                nowDownloading = true;
                btnStart.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CrossSafeSetMaximumMethod(Int32 value)
        {
            if (!prgInstall.Dispatcher.CheckAccess())
                prgInstall.Dispatcher.Invoke(cssm, value);
            else
                prgInstall.Maximum = value;
        }
        private void CrossSafeSetValueMethod(Int32 value)
        {
            if (!prgInstall.Dispatcher.CheckAccess())
                prgInstall.Dispatcher.Invoke(cssm, value);
            else
                prgInstall.Value = value;
        }

        private void MainFormLoad(object sender, RoutedEventArgs e)
        {
            DefaultProgressBarBrush = prgInstall.Foreground;
            VersionInfoUpdate();

            // 이벤트를 연결한다.
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(FileDownloadCompleted);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(FileDownloadProgressChanged);
        }

        void FileDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            // e.BytesReceived
            //   받은 데이터의 크기를 저장합니다.

            // e.TotalBytesToReceive
            //   받아야 할 모든 데이터의 크기를 저장합니다.

            // 프로그레스바의 최대 크기가 정해지지 않은 경우,
            // 받아야 할 최대 데이터 량으로 설정한다.
            if (!setBaseSize)
            {
                CrossSafeSetMaximumMethod((int)e.TotalBytesToReceive);
                setBaseSize = true;
            }

            // 받은 데이터 량을 나타낸다.
            CrossSafeSetValueMethod((int)e.BytesReceived);

            // 받은 데이터 / 받아야할 데이터 (퍼센트) 로 나타낸다.
            //CrossSafeSetTextMethod(String.Format("{0:N0} / {1:N0} ({2:P})", e.BytesReceived, e.TotalBytesToReceive, (Double)e.BytesReceived / (Double)e.TotalBytesToReceive));
        }

        void FileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            prgInstall.Foreground = DefaultProgressBarBrush;
            nowDownloading = false;
            btnStart.IsEnabled = true;

            MessageBox.Show("파일 다운로드 완료!", "오류", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VersionInfoUpdate();
            StartFileDownload(String.Format(@"https://teamhy.github.io/ChaosGreedier_{0}.7z", VersionUtility.LatestVersion), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Binding of Isaac Afterbirth+ Mods");
        }
    }
}
