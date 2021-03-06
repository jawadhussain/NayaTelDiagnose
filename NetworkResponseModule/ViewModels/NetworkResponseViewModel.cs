﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using NetworkResponseModule.Services;
using System.Collections.ObjectModel;
using NetworkResponseModule.Model;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;
using System.Windows;
using System.Net;
using System.Runtime.CompilerServices;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using Prism.Events;
using ViewSwitchingNavigation.Infrastructure.Events;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Diagnostics;
using static ViewSwitchingNavigation.Infrastructure.PropertyProvider;

namespace NetworkResponseModule.ViewModels
{
    [Export]
    public class NetworkResponseViewModel : NDoctorTest, INavigationAware , IDisposable
    {
        protected readonly IEventAggregator eventAggregator;
        Dictionary<String, String> infiDic;
         private readonly INetworkResponseService netWorkResponse;
        private readonly IRegionManager regionManager;
        private ObservableCollection<NetWorkResponse> netWorkResponseCollection1;
        private ObservableCollection<NetWorkResponse> netWorkResponseCollection2;
        private ObservableCollection<NetWorkResponse> netWorkResponseCollection3;
        private ObservableCollection<NetWorkResponse> netWorkResponseCollection4;
        private ObservableCollection<NetWorkResponse> netWorkResponseCollectionCustomMtr;
        String[] MTR_IPS_LIST = new String[] { "203.82.48.3", "115.186.188.3", "8.8.8.8","4.2.2.2" };


        private readonly DelegateCommand<object> startMtrCommand;

        public ICollectionView NetWorkResponses1 { get; private set; }
        public ICollectionView NetWorkResponses2 { get; private set; }
        public ICollectionView NetWorkResponses3 { get; private set; }
        public ICollectionView NetWorkResponses4 { get; private set; }
        public ICollectionView NetWorkResponsesCustomMtr { get; private set; }

        private bool _DisplayControlGridHeaderHide;
        public bool DisplayControlGridHeaderHide
        {
            get { return _DisplayControlGridHeaderHide; }

            set { SetProperty(ref _DisplayControlGridHeaderHide, value); }
        }
        private bool _CustomTextBoxHide;
        private bool _GridCustomMTRHide;
        public bool CustomTextBoxHide
        {
            get { return _CustomTextBoxHide; }

            set { SetProperty(ref _CustomTextBoxHide, value); }
        }
        //enable and dusable textBox
        private bool _CustomTextBoxEnable;
        public bool CustomTextBoxEnable
        {
            get { return _CustomTextBoxEnable; }

            set { SetProperty(ref _CustomTextBoxEnable, value); }
        }
        public bool GridCustomMTRHide
        {
            get { return _GridCustomMTRHide; }

            set { SetProperty(ref _GridCustomMTRHide, value); }
        }
        String _CustomIPAdress;
        String _CustomIterationInterval;
        String _CustomiterationsPerHost;
        String _CustomPacketSize;


        public String CustomIPAdress
        {
            get { return _CustomIPAdress; }

            set { SetProperty(ref _CustomIPAdress, value); }

        }
        public String CustomIterationInterval
        {
            get { return _CustomIterationInterval; }

            set { SetProperty(ref _CustomIterationInterval, value); }

        }
        public String CustomiterationsPerHost
        {
            get { return _CustomiterationsPerHost; }

            set { SetProperty(ref _CustomiterationsPerHost, value); }

        }
        public String CustomPacketSize
        {
            get { return _CustomPacketSize; }

            set { SetProperty(ref _CustomPacketSize, value); }

        }

        private bool _DisplayControlCoustomMtr;
        public bool DisplayControlCoustomMtr
        {
            get { return _DisplayControlCoustomMtr; }

            set { SetProperty(ref _DisplayControlCoustomMtr, value); }
        }
        private bool _DisplayControl1;
        public bool DisplayControl1
        {
            get { return _DisplayControl1; }

            set { SetProperty(ref _DisplayControl1, value); }
        }
        public bool _DisplayControl2;

        public bool DisplayControl2
        {
            get { return _DisplayControl2; }

            set { SetProperty(ref _DisplayControl2, value); }
        }
        public bool _DisplayControl3;
        public bool DisplayControl3
        {
            get { return _DisplayControl3; }

            set { SetProperty(ref _DisplayControl3, value); }
        }
        public bool _DisplayControl4;
        public bool DisplayControl4
        {
            get { return _DisplayControl4; }

            set { SetProperty(ref _DisplayControl4, value); }
        }
        private static Uri ViewUri = new Uri("/NetworkResponseView", UriKind.Relative);

        int indexHost = 0;
        bool isStop = false;
        int TestTimeout = 0;
        Boolean isEmail = true;
        BackgroundWorker barInvoker;
        PropertyProvider.WindowsMethod CurrentTestMethod;
        PropertyProvider.TestType CurrentTestType;
        Stopwatch _SW;
        [ImportingConstructor]
        public NetworkResponseViewModel(INetworkResponseService netWorkResponse, IRegionManager regionManager, IEventAggregator eventAggregator)
        {

            this.netWorkResponseCollection1 = new ObservableCollection<NetWorkResponse>();
            this.NetWorkResponses1 = new ListCollectionView(this.netWorkResponseCollection1);
            this.netWorkResponseCollection2 = new ObservableCollection<NetWorkResponse>();
            this.NetWorkResponses2 = new ListCollectionView(this.netWorkResponseCollection2);
            this.netWorkResponseCollection3 = new ObservableCollection<NetWorkResponse>();
            this.NetWorkResponses3 = new ListCollectionView(this.netWorkResponseCollection3);
            this.netWorkResponseCollection4 = new ObservableCollection<NetWorkResponse>();
            this.NetWorkResponses4 = new ListCollectionView(this.netWorkResponseCollection4);
            this.netWorkResponseCollectionCustomMtr = new ObservableCollection<NetWorkResponse>();
            this.NetWorkResponsesCustomMtr = new ListCollectionView(this.netWorkResponseCollectionCustomMtr);



            this.startMtrCommand = new DelegateCommand<object>(this.StartMtrPressed);

            this.netWorkResponse = netWorkResponse;
            this.regionManager = regionManager;
            this.PropertyChanged += NetworkResponseViewModel_PropertyChanged;
            this.GridCustomMTRHide = true;
            this.eventAggregator = eventAggregator;
            this.eventAggregator = eventAggregator;
            CurrentTestMethod = PropertyProvider.WindowsMethod.network_response;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);
            _SW = new Stopwatch();
         }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod)
            {
                if(!String.IsNullOrEmpty(this.CustomIPAdress))
                if (this.CustomIPAdress.Contains("(")) {
                    this.CustomIPAdress = this.CustomIPAdress.Substring(0,this.CustomIPAdress.IndexOf("("));
                }
                Dispose();
                StartMtrPressed(null);
            }
        }
        public ICommand StartMtrCommand
        {
            get { return this.startMtrCommand; }
        }
        private void StartMtrPressed(object ignored)
        {
            TestInfo info = new TestInfo();
            info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.network_response;
            info.testType = this.CurrentTestType;
            this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);

        }
        private void Initialize(String host, ObservableCollection<NetWorkResponse> netWorkResponseCollection, bool isCustomMtr)
        {

            barInvoker = new BackgroundWorker();
            barInvoker.WorkerSupportsCancellation = true;

            barInvoker.RunWorkerCompleted += delegate
            {
                if (!isStop && isCustomMtr)
                {
                    //stop Watch stop;
                    
                    _SW.Stop();
                    TestInfo info = new TestInfo();
                    infiDic = new Dictionary<string, string>();
                    infiDic.Add("time", "23");
                    info.infiDic = infiDic;
                    info.sender = this;
                    info.testMethod = PropertyProvider.WindowsMethod.network_response;
                    info.testType = CurrentTestType;
                    this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
                    
                }

                if (indexHost == 1 && !isStop && !isCustomMtr)
                {
                    DisplayControl2 = true;

                    this.Initialize(MTR_IPS_LIST[1], netWorkResponseCollection2, false);

                }
                else if (indexHost == 2 && !isStop && !isCustomMtr)
                {
                    DisplayControl3 = true;

                    this.Initialize(MTR_IPS_LIST[2], netWorkResponseCollection3, false);

                }
                else if (indexHost == 3 && !isStop && !isCustomMtr)
                {
                    DisplayControl4 = true;

                    this.Initialize(MTR_IPS_LIST[3], netWorkResponseCollection4, false);

                } else if ((indexHost == 4 && !isStop)) {
                    // Stop watch
                    _SW.Stop();
                    TestInfo info = new TestInfo();
                    infiDic = new Dictionary<string, string>();
                    infiDic.Add("time", "23");
                    info.infiDic = infiDic;
                    info.sender = this;
                    info.testMethod = PropertyProvider.WindowsMethod.network_response;
                    info.testType = CurrentTestType;
                    this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
                    
                }
            };
            barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.10));

                int max_ttl = PropertyProvider.getPropertyProvider().getMTRMaxTtL();
                MTR_Config_Values Config_iteration_interval = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationInterval);
                MTR_Config_Values Config_iterations_per_host = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationsPerHost);
                MTR_Config_Values Config_packet_size = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.PacketSize);
                hop_discovery_Timout  Hop_Descovery =  PropertyProvider.getPropertyProvider().hopDiscovery();
                float hopTimeout = Hop_Descovery.Default;
                if (isCustomMtr)
                    hopTimeout = Hop_Descovery.custom;

                int iteration_interval = Config_iteration_interval.Default;
                int iterations_per_host = Config_iterations_per_host.Default;
                int packet_size = Config_packet_size.Default;

                if (  CurrentTestType != PropertyProvider.TestType.DiagnoseAll)
                {
                    int converstion = 0;
                    if (int.TryParse(this.CustomPacketSize, out converstion))
                        packet_size = converstion ;     
                    if (int.TryParse(this.CustomIterationInterval, out converstion))
                        iteration_interval = converstion;
                    if (int.TryParse(this.CustomiterationsPerHost, out converstion))
                        iterations_per_host = converstion;
                }



                int pingtimout = (CurrentTestType == PropertyProvider.TestType.DiagnoseAll) ? Constants.DIAGNOSEALL_PING_TIME_OUT : Constants.INDIVIDUAL_PING_TIME_OUT;
                netWorkResponse.GetNetWorkResponse(host, hopTimeout,TestTimeout, pingtimout, max_ttl, iteration_interval, iterations_per_host, packet_size, (IEnumerable<IPAddress> routes1) =>
                {
                    foreach (var item in routes1)
                    {
                        NetWorkResponse response = new NetWorkResponse();
                        if (item == null)
                        {
                            response.IPAdress = "*";
                            response.Last = "*";
                            response.Rec = "*";
                            response.Loss = "*";
                            response.Avg = "*";
                            response.Best = "*";
                            response.Worst = "*";
                            response.Sent = "*";
                        }
                        else {
                            response.IPAdress = item.ToString();
                            response.Last = "0";
                            response.Rec = "0";
                            response.Loss = "0";
                            response.Avg = "0";
                            response.Best = "0";
                            response.Worst = "0";
                            response.Sent = "0";
                        }
                       
                        //mtrDictionary.Add(item.ToString(), response);

                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            netWorkResponseCollection.Add(response);
                        }));

                    }





                }, (NetWorkResponse mtr) =>
             {


                 Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                 {


                     // if (netWorkResponseCollection[i].IPAdress == mtr.IPAdress)
                     //{
                     if (netWorkResponseCollection.Count() > 0)
                     {
                         if (mtr.Loss.Contains(".")) {
                             mtr.Loss = mtr.Loss.Substring(0,mtr.Loss.IndexOf("."));

                         }
                         netWorkResponseCollection.RemoveAt(mtr.Index);
                         netWorkResponseCollection.Insert(mtr.Index, mtr);


                     }


                     //}








                 }));
             });

            };

            barInvoker.RunWorkerAsync();
            indexHost++;


        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Dispose();

            this.GridCustomMTRHide = true;

            DisplayControlGridHeaderHide = false;
            if (navigationContext.Parameters.Count() > 0)
            {
                Object testType1 = navigationContext.Parameters[TestInfo.ParmKey];
                this.CurrentTestType = (PropertyProvider.TestType)testType1;
            }
            else
            {
                this.CurrentTestType = PropertyProvider.TestType.Individual;

                MTR_Config_Values Config_iteration_interval = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationInterval);
                MTR_Config_Values Config_iterations_per_host = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationsPerHost);
                MTR_Config_Values Config_packet_size = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.PacketSize);



                int iteration_interval = Config_iteration_interval.Default;
                int iterations_per_host = Config_iterations_per_host.Default;
                int packet_size = Config_packet_size.Default;

                this.CustomPacketSize = packet_size.ToString();
                     this.CustomIterationInterval   = iteration_interval.ToString();
                     this.CustomiterationsPerHost = iterations_per_host.ToString();
                


            }
            if (this.CurrentTestType == PropertyProvider.TestType.DiagnoseAll)
            {
                // indexHost = 0;
                // isStop = false;
                // DisplayControlGridHeaderHide = true;
                // this.GridCustomMTRHide = false;
                //  var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.network_response, this.CurrentTestType);
                //  TestTimeout = tuple.Item1 * 1000 / 4;
                // DisplayControl1 = true;
                //this.Initialize(MTR_IPS_LIST.First(), netWorkResponseCollection1, false);
                  StartMtrPressed(null);
            }



        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {

            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {


           this.CustomIPAdress = "";
            Dispose();







        }

        public void cancelTest()
        {
            isStop = true;
        }

        private void NetworkResponseViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CustomIPAdress")
            {
                if (CustomIPAdress.Length > 0)
                {
                    this.CustomTextBoxHide = true;
                    this.CustomTextBoxEnable = true;
                }
                else
                {
                    this.CustomTextBoxHide = false;
                    this.CustomTextBoxEnable = false;


                }
            }
            //if(CustomIPAdress.Length>0)

        }

        public void Dispose()
        {
            
            cancelTest();
            this.netWorkResponse.StopMtr();
            if (barInvoker != null)
            {
                barInvoker.CancelAsync();
            }
            netWorkResponseCollection1.Clear();
            netWorkResponseCollection2.Clear();
            netWorkResponseCollection3.Clear();
            netWorkResponseCollection4.Clear();
            netWorkResponseCollectionCustomMtr.Clear();

            DisplayControl1 = false;
            DisplayControl2 = false;
            DisplayControl3 = false;
            DisplayControl4 = false;
            DisplayControlCoustomMtr = false;
            this.GridCustomMTRHide = false;
        }

        public override void StopTest()
        {
            Dispose();
         }

         
        public override void StartTest()
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.network_response, this.CurrentTestType);

            //For Diagnose all test
            if (this.CurrentTestType == PropertyProvider.TestType.DiagnoseAll)
            {
                indexHost = 0;
                isStop = false;
                DisplayControlGridHeaderHide = true;
                this.GridCustomMTRHide = false;
                TestTimeout = tuple.Item1 * 1000 / 4;
                DisplayControl1 = true;
                this.Initialize(MTR_IPS_LIST.First(), netWorkResponseCollection1, false);
                return;
             }


            IPAddress CustomIP;
 
            string host = null;
            if (!IPAddress.TryParse(this.CustomIPAdress, out CustomIP) && !String.IsNullOrEmpty(this.CustomIPAdress)) {
                try
                {
                     IPAddress[] addresslist = Dns.GetHostAddresses(this.CustomIPAdress.Trim());
                    if (addresslist.Count()>0)
                    {
                        host =  addresslist.First().ToString();
                    }
                }
                catch (Exception)
                {
                    DisplayMessage message = new DisplayMessage();
                    message.displayScren = DisplayMessage.DisplayScreen.Box;
                    message.message = Constants.MESSAGES_INPUT_INVALID_HOST_NAME.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, this.CustomIPAdress);
                    message.messageTittle = getTestTittle();
                    this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);


                     testFaild();


                    return;
                }
               

            }


           
            // validation iteration, intervals,packet size

            if (!UtilValidation.isNumber(this.CustomiterationsPerHost))
            {
                testFaild();
                 DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_INVALID_INTERVAL.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, this.CustomiterationsPerHost);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);

                return;
            }


            else if (!UtilValidation.isNumber(this.CustomIterationInterval))
            {
                testFaild();
                DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_INVALID_ITERATION.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, this.CustomIterationInterval);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);

                 return;

            }
            else if (!UtilValidation.isNumber(this.CustomPacketSize)) {
                testFaild();
                DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_INVALID_PACKET_SIZE.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, this.CustomPacketSize);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);
                return;
            }


            //////// Check max and min value mtr custom field

            MTR_Config_Values Config_iteration_interval = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationInterval);
            MTR_Config_Values Config_iterations_per_host = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationsPerHost);
            MTR_Config_Values Config_packet_size = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.PacketSize);

            if (Config_iterations_per_host.Min > int.Parse(this.CustomiterationsPerHost) || Config_iterations_per_host.Max < int.Parse(this.CustomiterationsPerHost))
            {
                testFaild();
                DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_MIN_MAX_ITERATION.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, Config_iterations_per_host.Min +" AND "+ Config_iterations_per_host.Max);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);

                return;
            }
            if (Config_iteration_interval.Min > int.Parse(this.CustomIterationInterval) || Config_iteration_interval.Max < int.Parse(this.CustomIterationInterval))
            {
                testFaild();
                DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_MIN_MAX_ITERATION.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, Config_iteration_interval.Min + " AND " + Config_iteration_interval.Max);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);

                return;
            }
            if (Config_packet_size.Min > int.Parse(this.CustomPacketSize) || Config_packet_size.Max < int.Parse(this.CustomPacketSize))
            {
                testFaild();
                DisplayMessage message = new DisplayMessage();
                message.displayScren = DisplayMessage.DisplayScreen.Box;
                message.message = Constants.MESSAGES_INPUT_MIN_MAX_ITERATION.Replace(Constants.MESSAGES_INPUT_REPLACE_VALUE, Config_packet_size.Min + " AND " + Config_packet_size.Max);
                message.messageTittle = getTestTittle();
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Publish(message);

                return;
            }






            ////////////////////////// Test Starting
            isEmail = true;
            indexHost = 0;
            isStop = false;
            DisplayControlGridHeaderHide = true;
            this.GridCustomMTRHide = false;
            //check total time out for testing
            _SW.Reset();
            _SW.Start();
            if (IPAddress.TryParse(this.CustomIPAdress, out CustomIP) || !String.IsNullOrEmpty(host))
            {
                DisplayControlCoustomMtr = true;
                netWorkResponseCollectionCustomMtr.Clear();



                TestTimeout = int.Parse( CustomIterationInterval) * int.Parse(CustomiterationsPerHost);
                String ip = (!String.IsNullOrEmpty(host)) ? host: CustomIP.ToString();
                this.CustomIPAdress += (!String.IsNullOrEmpty(host)) ? " ("+ip+")" : "";
                this.Initialize(ip, netWorkResponseCollectionCustomMtr, true);

            } 
            else
            {
                DisplayControl1 = true;
                TestTimeout = tuple.Item1 * 1000 / 4;
                this.Initialize(MTR_IPS_LIST.First(), netWorkResponseCollection1, false);

            }


        }

        public override Boolean IsEmail()
        {
            return isEmail;

        }

        public override String emailBody()
        {
            String results =  genrateEmailBody();
            return results;
        }

        String genrateEmailBodyDetails()
        {
            String table = ""


                          + Constants.HTML_START_TABLE +

                           Constants.HTML_START_TR +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Intervals") +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, CustomIterationInterval ) +
                           Constants.HTML_END_TR + Constants.HTML_START_TR +
                            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Iterations") +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, CustomiterationsPerHost) +

                           Constants.HTML_END_TR + Constants.HTML_START_TR +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Packet Size") +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, CustomPacketSize) +
                            Constants.HTML_END_TR + Constants.HTML_START_TR +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Complete Test Time") +
                           Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, _SW.Elapsed.Seconds.ToString()) +

                           Constants.HTML_END_TR;



            table += Constants.HTML_END_TABLE;

            return table;
        }

        String genrateEmailBody()
        {

       



            String table = ""; 
             String headerTR =  

                             Constants.HTML_START_TR+
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Host Name") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Loss %") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Sent Packets") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Received Packets") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Best (ms)") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Average (ms)") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Worst (ms)") +
                             Constants.HTML_END_TR;

            //if custom mtr


            Dictionary<String,ObservableCollection<NetWorkResponse>> DiscTablelist = new Dictionary<String, ObservableCollection<NetWorkResponse>>();
            if (String.IsNullOrEmpty(CustomIPAdress))
            {
                DiscTablelist.Add(MTR_IPS_LIST[0], netWorkResponseCollection1);
                DiscTablelist.Add(MTR_IPS_LIST[1], netWorkResponseCollection2);
                DiscTablelist.Add(MTR_IPS_LIST[2], netWorkResponseCollection3);
                DiscTablelist.Add(MTR_IPS_LIST[3], netWorkResponseCollection4);
            }

            if (!String.IsNullOrEmpty(CustomIPAdress)) {
                if (DiscTablelist.ContainsKey(CustomIPAdress))
                    DiscTablelist[CustomIPAdress] = netWorkResponseCollectionCustomMtr;
                else
                    DiscTablelist.Add(CustomIPAdress, netWorkResponseCollectionCustomMtr);
            }


            foreach (var item in DiscTablelist)
            {
                table += Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, item.Key);
                table += Constants.HTML_START_TABLE + headerTR;
                foreach (var item1 in item.Value)
                {
                    NetWorkResponse response = item1;
                     
                        table += Constants.HTML_START_TR +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.IPAdress) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Loss) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Sent) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Rec) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Best) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Avg) +
                    Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, response.Worst) +
                    Constants.HTML_END_TR;
                    }
                    table += Constants.HTML_END_TABLE;

                }
              

            //this for 8.8.8.8
          // // if (netWorkResponseCollection1.Count() > 0)
           // {
               
           // }//end if 8.8.8.8
           


           
            return table;
        }

        public override string getTestTittleHTMLTable()
        {
            //Tittle header
          String  TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, "Network Response");
            //user info like mac ip email getUserHTMLTable

            return TittleTable;
        }
        public override string getTestTittle()
        {
            return Constants.TEST_HEADER_TITTLE_NETWORK_RESPONSE;
        }
        void testFaild() {
            TestInfo info = new TestInfo();
            infiDic = new Dictionary<string, string>();
            infiDic.Add("time", "23");
            info.infiDic = infiDic;
            info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.network_response;
            info.testType = CurrentTestType;
            this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
            isEmail = false;

        }
    }

     
}