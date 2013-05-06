using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ANYCHATAPI;
using System.IO;
using System.Threading;
using System.Windows.Automation;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;

namespace AnyChatforWPF
{
    public partial class Window1 : Window
    {
        static AnyChatCoreSDK.NotifyMessage_CallBack OnNotifyMessageCallback = new AnyChatCoreSDK.NotifyMessage_CallBack(NotifyMessage_CallBack);
        static AnyChatCoreSDK.VideoData_CallBack OnVideoDataCallback = new AnyChatCoreSDK.VideoData_CallBack(VideoData_CallBack);
        static AnyChatCoreSDK.TextMessage_CallBack OnTextMessageCallBack = new AnyChatCoreSDK.TextMessage_CallBack(TextMessage_CallBack);//TxtMsg Added

        public static AnyChatCoreSDK.NotifyMessage_CallBack NotifyMessageHandler = null;
        public static AnyChatCoreSDK.VideoData_CallBack VideoDataHandler = null;
        public static AnyChatCoreSDK.TextMessage_CallBack TextMessageHandler = null; //TxtMsg Added

        public static int g_selfUserId = -1;
        public static int g_otherUserId = -1;
        public Point mousePointSent;
        public Point mousePointSentCopy;
        public Point mousePointReceived;

        //Drawing Related
        private double a, b;

        public Point point1;
        public Point point2;

        public TouchPoint touchPointSent;
        public TouchPoint touchPointSentCopy;
        public TouchPoint currentTouchPoint;
        public TouchPoint startPointTouch;
        private string[] coord;
        private int[] coordNum;
        public int[] numberInt;
        private Point startPoint;


        public Point currentPoint;
        public Point currentPointCopy;

        public Polyline line;
        public Polyline lineCopy;

        public Point movingPoint;
        public Point movingPointCopy;

        public int n;

        public PointCollection pc;
        public Point startPointCopy;
        //SqlConnectionStringBuilder csBuilder = new SqlConnectionStringBuilder();
        InfoController _controller = new InfoController();
        int chatroom = 1;
        
        public Window1()
        {
            
            InitializeComponent();
            LoginWindow(); //display login window first - user inputs login id or username to create a new user
            testlabel.Content = "Hello " + _controller.username + ", your ID is" + _controller.idnum;
            FillFriendGrid();
            //StartVideo(chatroom);
            

        }
        public void LoginWindow()
        {
            Login myWindow = new Login(_controller);
            myWindow.ShowDialog();

        }
        private void StartVideo(int roomNo)
        {
           // testlabel.Content = "ID:" + Convert.ToString(_controller.idnum) + "   username:" + _controller.username;
            AnyChatCoreSDK.SetNotifyMessageCallBack(OnNotifyMessageCallback, 0);
            AnyChatCoreSDK.SetVideoDataCallBack(AnyChatCoreSDK.PixelFormat.BRAC_PIX_FMT_RGB24, OnVideoDataCallback, 0);
            AnyChatCoreSDK.SetTextMessageCallBack(OnTextMessageCallBack, 0); //TxtMsg Added

            ulong dwFuncMode = AnyChatCoreSDK.BRAC_FUNC_VIDEO_CBDATA | AnyChatCoreSDK.BRAC_FUNC_AUDIO_AUTOPLAY | AnyChatCoreSDK.BRAC_FUNC_CHKDEPENDMODULE
                | AnyChatCoreSDK.BRAC_FUNC_AUDIO_VOLUMECALC | AnyChatCoreSDK.BRAC_FUNC_NET_SUPPORTUPNP | AnyChatCoreSDK.BRAC_FUNC_FIREWALL_OPEN
                | AnyChatCoreSDK.BRAC_FUNC_AUDIO_AUTOVOLUME | AnyChatCoreSDK.BRAC_FUNC_CONFIG_LOCALINI;


            AnyChatCoreSDK.InitSDK(IntPtr.Zero, dwFuncMode);
            /////////////////////////
            /////Need to Modify//////
            /////////////////////////
            AnyChatCoreSDK.Connect("128.197.180.243", 8906); //demo.anychat.cn
            AnyChatCoreSDK.Login("Test2", "", 0);
            if (roomNo%6 == 0)
                roomNo = 1; 
            AnyChatCoreSDK.EnterRoom(roomNo%6, "", 0);
            testlabel.Content = "Entering Room " + roomNo + ", " + roomNo%6 + ", " + (roomNo%6)+1;
            NotifyMessageHandler = new AnyChatCoreSDK.NotifyMessage_CallBack(NotifyMessageCallbackDelegate);
            VideoDataHandler = new AnyChatCoreSDK.VideoData_CallBack(VideoDataCallbackDelegate);
            TextMessageHandler = new AnyChatCoreSDK.TextMessage_CallBack(TextMessageCallbackDelegate); //TxtMsg Added

        }
        private void FillFriendGrid()
        {
            DataTable dt = new DataTable("Users");
            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string CmdString = string.Empty;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                CmdString = "select friend_id from friends, online where friends.friend_id = online.user_id and friends.id = @u_uid";
                //CmdString = "select username from friends, users, online where friends.friend_id = online.user_id and friends.id = @f_uid users.id = @u_uid";
                SqlCommand cmd = new SqlCommand(CmdString, con);
                //cmd.Parameters.AddWithValue("@f_uid", _controller.idnum);
                cmd.Parameters.AddWithValue("@u_uid", _controller.idnum);
                //testlabel.Content = CmdString;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);

                sda.Fill(dt);
                TestGrid.ItemsSource = dt.DefaultView;
            }
        }
        private void AddFriendEvents()
        {
            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection friend_db = new SqlConnection(ConString);
            string friendCheck = "SELECT * FROM friends WHERE id = @idnum and friend_id = @friend_id";

            friend_db.Open();
            using (friend_db)
            {
                string sqlCheck = "SELECT id FROM users WHERE id = @idnum";
                SqlCommand cmdChk = new SqlCommand(sqlCheck, friend_db);
                cmdChk.Parameters.AddWithValue("@idnum", fid_text.Text);
                object result = cmdChk.ExecuteScalar();
                cmdChk.Parameters.Clear();
                if (result != DBNull.Value && result != null) //if user can not be found
                {
                    SqlCommand cmdChk2 = new SqlCommand(friendCheck, friend_db);
                    cmdChk2.Parameters.AddWithValue("@idnum", _controller.idnum);
                    cmdChk2.Parameters.AddWithValue("@friend_id", Convert.ToInt32(fid_text.Text));
                    object result2 = cmdChk2.ExecuteScalar();
                    cmdChk2.Parameters.Clear();
                    if (result2 != DBNull.Value && result != null) //if there is no friend listing with this combo
                    {
                        testlabel.Content = "You are already friends with this user.";
                    }
                    else
                    {

                        string sqlIns = "INSERT INTO friends (ID, Friend_ID) VALUES (@idnum, @friend_id)";
                        // friend_db.Open();
                        testlabel.Content = "your ID: " + _controller.idnum;
                        SqlCommand cmdIns = new SqlCommand(sqlIns, friend_db);
                        cmdIns.Parameters.AddWithValue("@idnum", _controller.idnum);
                        cmdIns.Parameters.AddWithValue("@friend_id", Convert.ToInt32(fid_text.Text));
                        cmdIns.ExecuteNonQuery();
                        cmdIns.Parameters.Clear();
                        testlabel.Content = "You have added user " + fid_text.Text;
                    }
                }
                else
                {
                    testlabel.Content = "This user does not exist.";
                }
                friend_db.Close();
                //  }
                //  catch (Exception ex)
                //   {
                //      testlabel.Content = ex.InnerException.ToString();
                //  }
                FillFriendGrid();
            }
        }

        private void addFriend_button_Click(object sender, RoutedEventArgs e)
        {
            AddFriendEvents();
        }

        private void addFriend_button_TouchDown(object sender, TouchEventArgs e)
        {
            AddFriendEvents();
        }
        private void ConnectTouchClickEvents() //handler for both touch and mouse events
        {
            FriendList.Visibility = Visibility.Hidden;
            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection db = new SqlConnection(ConString);
            string sqlCheck = "SELECT id FROM friends WHERE id = @uid and friend_id = @fid ";
            db.Open();
            using (db)
            {
                SqlCommand cmdCheckFriend = new SqlCommand(sqlCheck, db);
                cmdCheckFriend.Parameters.AddWithValue("@uid", _controller.idnum);
                cmdCheckFriend.Parameters.AddWithValue("@fid", Convert.ToInt32(connect_txt.Text));
                object result = cmdCheckFriend.ExecuteScalar();

                if (result != DBNull.Value && result != null) //if the entry exists (friends)
                {
                    string checkChat = "SELECT UID_1 from tochat where UID_1 = @fid and UID_2 = @uid"; //check if friend has already made room
                    SqlCommand tochatChk = new SqlCommand(checkChat, db);
                    tochatChk.Parameters.AddWithValue("@fid", Convert.ToInt32(connect_txt.Text));
                    tochatChk.Parameters.AddWithValue("@uid", _controller.idnum);
                    object checkExist = tochatChk.ExecuteScalar();
                    if (checkExist != DBNull.Value && checkExist != null) //if the entry exists (friend made room already)
                    {
                        string roomQry = "SELECT id FROM tochat where UID_1 = @fid AND UID_2 = @uid";
                        SqlCommand getRoom = new SqlCommand(roomQry, db);
                        getRoom.Parameters.AddWithValue("@uid", _controller.idnum);
                        getRoom.Parameters.AddWithValue("@fid", Convert.ToInt32(connect_txt.Text));
                        chatroom = Convert.ToInt32(getRoom.ExecuteScalar());
                        getRoom.Parameters.Clear();
                    }
                    else
                    {
                        string chatIns = "INSERT INTO tochat (UID_1, UID_2) VALUES (@uid, @fid)";
                        SqlCommand tochatIns = new SqlCommand(chatIns, db);
                        tochatIns.Parameters.AddWithValue("@uid", _controller.idnum);
                        tochatIns.Parameters.AddWithValue("@fid", Convert.ToInt32(connect_txt.Text));
                        tochatIns.ExecuteNonQuery();
                        tochatIns.Parameters.Clear();

                        string roomQry = "SELECT id FROM tochat where UID_1 = @uid AND UID_2 = @fid";
                        SqlCommand getRoom = new SqlCommand(roomQry, db);
                        getRoom.Parameters.AddWithValue("@uid", _controller.idnum);
                        getRoom.Parameters.AddWithValue("@fid", Convert.ToInt32(connect_txt.Text));
                        chatroom = Convert.ToInt32(getRoom.ExecuteScalar());
                        getRoom.Parameters.Clear();
                    }
                }
                db.Close();
            }
            StartVideo(chatroom);
            
        }

        private void Connectbutton_Click(object sender, RoutedEventArgs e)
        {
            ConnectTouchClickEvents();
        }
        private void Connectbutton_TouchDown(object sender, TouchEventArgs e)
        {
            ConnectTouchClickEvents();
            TestGrid.CurrentItem.ToString();
        }
        private void EndChatEvents()
        {
            AnyChatCoreSDK.LeaveRoom(-1);
            AnyChatCoreSDK.Logout();
            AnyChatCoreSDK.Release();

            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection db = new SqlConnection(ConString);
            string useroff = "DELETE FROM online WHERE User_ID = @logoff_id";
            string chatoff = "DELETE FROM tochat WHERE ID = @room_id";
            db.Open();
            using (db)
            {
                SqlCommand cmdIns = new SqlCommand(useroff, db);
                cmdIns.Parameters.AddWithValue("@logoff_id", _controller.idnum);
                cmdIns.ExecuteNonQuery();
                cmdIns.Parameters.Clear();

                SqlCommand chatDel = new SqlCommand(chatoff, db);
                chatDel.Parameters.AddWithValue("@room_id", chatroom);
                chatDel.ExecuteNonQuery();
                chatDel.Parameters.Clear();
            }
            db.Close();
        } //handler for all chatroom closing events
        private void Disconnectbutton_Click(object sender, RoutedEventArgs e)
        {
            remoteVideoImage.Source = null;
            localVideoImage.Source = null;
            drawPanel.Children.Clear();
            displayPanel.Children.Clear();
            FriendList.Visibility = Visibility.Visible;
            EndChatEvents();
        }
        private void Disconnectbutton_TouchDown(object sender, TouchEventArgs e)
        {
            remoteVideoImage.Source = null;
            localVideoImage.Source = null;
            FriendList.Visibility = Visibility.Visible;
            EndChatEvents();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EndChatEvents();
        }


        // AnyChatCore Call Back, do not touch
        public static void NotifyMessage_CallBack(int dwNotifyMsg, int wParam, int lParam, int userValue)
        {
            if (NotifyMessageHandler != null)
                NotifyMessageHandler(dwNotifyMsg, wParam, lParam, userValue);
        }
        // Setting Notify Message
        public void NotifyMessageCallbackDelegate(int dwNotifyMsg, int wParam, int lParam, int userValue)
        {

            switch (dwNotifyMsg)
            {
                case AnyChatCoreSDK.WM_GV_CONNECT:
                    if (wParam != 0)
                    {
                        msglabel.Content = "Connected to Server Successfully!";
                        label1.Visibility = Visibility.Hidden;
                        label2.Visibility = Visibility.Hidden;
                        msglabel.Visibility = Visibility.Hidden;
                    }
                    else
                        msglabel.Content = "Failed to Connected to the server!";
                    break;
                case AnyChatCoreSDK.WM_GV_LOGINSYSTEM:
                    if (lParam == 0)
                    {
                        g_selfUserId = wParam;
                        msglabel.Content = "Log In successfully!";
                    }
                    else
                    {
                        msglabel.Content = "Login failed, ErrorCode:" + lParam;
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_ENTERROOM:
                    if (lParam == 0)
                    {
                        msglabel.Content = "Enter Room successfully!";
                        AnyChatCoreSDK.UserSpeakControl(-1, true);
                        AnyChatCoreSDK.UserCameraControl(-1, true);
                    }
                    else
                        msglabel.Content = "Enter Room failed, ErrorCode:" + lParam;
                    break;
                case AnyChatCoreSDK.WM_GV_ONLINEUSER:
                    OpenRemoteUserVideo();
                    break;
                case AnyChatCoreSDK.WM_GV_USERATROOM:
                    if (lParam != 0)     // 其它用户进入房间
                    {
                        test.Text = "get";
                        OpenRemoteUserVideo();
                        //test.Text = lParam.ToString();
                    }
                    else                // 其它用户离开房间
                    {
                        if (wParam == g_otherUserId)
                        {
                            g_otherUserId = -1;
                            OpenRemoteUserVideo();
                        }
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_LINKCLOSE:
                    msglabel.Content = "No Internet Connection!, ErrorCode:" + lParam;
                    break;
                default:
                    break;
            }
        }
        // AnyChat VideoData Callback
        public static void VideoData_CallBack(int userId, IntPtr buf, int len, AnyChatCoreSDK.BITMAPINFOHEADER bitMap, int userValue)
        {
            if (VideoDataHandler != null)
                VideoDataHandler(userId, buf, len, bitMap, userValue);
        }
        // Static Video Callback
        public void VideoDataCallbackDelegate(int userId, IntPtr buf, int len, AnyChatCoreSDK.BITMAPINFOHEADER bitMap, int userValue)
        {
            int stride = bitMap.biWidth * 3;
            BitmapSource bs = BitmapSource.Create(bitMap.biWidth, bitMap.biHeight, 96, 96, PixelFormats.Bgr24, null, buf, len, stride);
            // 将图像进行翻转
            TransformedBitmap RotateBitmap = new TransformedBitmap();
            RotateBitmap.BeginInit();
            RotateBitmap.Source = bs;
            RotateBitmap.Transform = new RotateTransform(180);
            RotateBitmap.EndInit();
            RotateBitmap.Freeze();

            // 异步操作
            Action action = new Action(delegate()
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    if (userId == g_selfUserId)
                        localVideoImage.Source = RotateBitmap;
                    else if (userId == g_otherUserId)
                        remoteVideoImage.Source = RotateBitmap;
                }), null);
            });
            action.BeginInvoke(null, null);
        }
        // Open Remote User Video
        public void OpenRemoteUserVideo()
        {
            if (g_otherUserId != -1)
                return;
            // 获取当前房间用户列表
            int usercount = 0;
            AnyChatCoreSDK.GetOnlineUser(null, ref usercount);
            if (usercount > 0)
            {
                int[] useridarray = new int[usercount];
                AnyChatCoreSDK.GetOnlineUser(useridarray, ref usercount);
                for (int i = 0; i < usercount; i++)
                {
                    // 判断该用户的视频是否已打开
                    int usercamerastatus = 0;
                    if (AnyChatCoreSDK.QueryUserState(useridarray[i], AnyChatCoreSDK.BRAC_USERSTATE_CAMERA, ref usercamerastatus, sizeof(int)) != 0)
                        continue;
                    //camera status is not open usercamerastatus==1
                    if (usercamerastatus == 2 || usercamerastatus == 1)
                    {

                        AnyChatCoreSDK.UserSpeakControl(useridarray[i], true);
                        AnyChatCoreSDK.UserCameraControl(useridarray[i], true);
                        g_otherUserId = useridarray[i];
                        break;
                    }
                }
            }
        }


        /*
        * Added for Sending Message Use
        * 
        */
        private static void TextMessage_CallBack(int fromuserId, int touserId, bool isserect, string message, int len, int userValue)
        {
            if (TextMessageHandler != null)
                TextMessageHandler(fromuserId, touserId, isserect, message, len, userValue);
        }

        //Added for testing
        void Print(string msg)
        {
            TbxAccept.Text += msg + "\r\n";
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = TbxSend.Text;
            int length = UnicodeEncoding.Default.GetBytes(message).Length;
            int ret = AnyChatCoreSDK.SendTextMessage(-1, false, message, length);
        }

        //If remote user has mouse down, then send "start" message, then local user display canvas starts to draw
        //If remote user clicked "clear" button, then send "clear" message, and erase both screens.
        void TextMessageCallbackDelegate(int fromuserId, int touserId, bool isserect, string message, int len, int userValue)
        {
            point2 = new Point(0, 0);
            if (message.Equals("Clear"))
            {
                clearDueToRemote();

            }
            else if (message.Equals("mouseUp"))
            {

                startPointCopy = point2;

            }


            numberInt = createPoint(message);
            pc = new PointCollection();
            draw(numberInt);

        }


        /*
         * Added for Drawing
         * 
         */
        public void draw(int[] coord)
        {
            a = coord[0];
            b = coord[1];


            point1 = new Point(a, b);
            lineCopy = new Polyline();

            lineCopy.Stroke = new SolidColorBrush(Colors.Red);
            lineCopy.StrokeThickness = 3.0;

            pc.Add(startPointCopy);
            pc.Add(point1);

            lineCopy.Points = pc;

            if (startPointCopy != point2)
            {
                displayPanel.Children.Add(lineCopy);

            }
            startPointCopy = point1;
        }



        public int[] createPoint(string message)
        {
            coordNum = new int[2];
            coord = Regex.Split(message, @",");
            coordNum[0] = Convert.ToInt32(coord[0]);
            coordNum[1] = Convert.ToInt32(coord[1]);

            return coordNum;
        }

        //Erase Paint
        private void clearPanel(object sender, RoutedEventArgs e)
        {
            TbxSend.Text = "Clear";
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            this.drawPanel.Children.Clear();
            this.displayPanel.Children.Clear();
        }

        public void clearDueToRemote()
        {
            this.displayPanel.Children.Clear();
            this.drawPanel.Children.Clear();
        }

        //MouseDown
        private void drawPanelMouseDown(object sender, MouseButtonEventArgs e)
        {

            mousePointSent = Mouse.GetPosition(drawPanel);
            mousePointSentCopy = Mouse.GetPosition(drawPanel);//Send Test Added



            startPoint = mousePointSent;
            line = new Polyline();
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 3.0;
            drawPanel.Children.Add(line);

            TbxSend.Text = "start";
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));



        }

        //MouseMove
        private void drawPanelMouseMove(object sender, MouseEventArgs e)
        {
            mousePointSent = Mouse.GetPosition(drawPanel);
            mousePointReceived = Mouse.GetPosition(displayPanel);


            if (e.LeftButton == MouseButtonState.Pressed)
            {

                currentPoint = e.GetPosition(drawPanel);
                if (startPoint != currentPoint && currentPoint != null)
                    line.Points.Add(currentPoint);


                TbxSend.Text = currentPoint.ToString();
                TbxAccept.Text = line.ToString();
                sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            }


        }

        private void drawPanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            TbxSend.Text = "mouseUp";
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void drawPanel_TouchDown(object sender, TouchEventArgs e)
        {
            touchPointSent = e.GetTouchPoint(drawPanel);
            touchPointSentCopy = e.GetTouchPoint(drawPanel);


            startPointTouch = touchPointSent;
            line = new Polyline();
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 3.0;
            drawPanel.Children.Add(line);

            TbxSend.Text = "start";
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void drawPanel_TouchMove(object sender, TouchEventArgs e)
        {
            touchPointSent = e.GetTouchPoint(drawPanel);
            touchPointSentCopy = e.GetTouchPoint(drawPanel);
            mousePointReceived = Mouse.GetPosition(displayPanel);


            currentTouchPoint = e.GetTouchPoint(drawPanel);

            if (startPointTouch != currentTouchPoint && currentTouchPoint != null)
                line.Points.Add(currentTouchPoint.Position);


            TbxSend.Text = currentTouchPoint.Position.ToString();
            TbxAccept.Text = line.ToString();
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void drawPanel_TouchUp(object sender, TouchEventArgs e)
        {
            TbxSend.Text = "mouseUp";
            sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void TestGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int selectedFriend = (int)TestGrid.SelectedValue;
            testlabel.Content = "selected friend id: " + Convert.ToString(selectedFriend);
        }

        private void FriendList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int selection = Convert.ToInt32(TestGrid.SelectedItem.ToString());
        }











    }
}
