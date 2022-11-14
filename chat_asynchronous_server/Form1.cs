using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Text;
using System.Dynamic;

namespace chat_asynchronous_server
{
    //ChatBox = new System.Windows.Forms.TextBox();
    //ServerStatus = new System.Windows.Forms.Label();
    //ServerOnOffBtn = new System.Windows.Forms.Button();

    public partial class ChatServerForm : Form
    {
        //UI 쓰레드에게 일을 넘기기위한 대리자
        delegate void SetTextDelegate(string s);
        Socket mainSocket;
        //로컬호스트 주소, 고정 포트번호 사용
        IPAddress thisAddress = IPAddress.Loopback;
        int port = 2022;
        int MAX_BYTE = 4096;
        //서버 온 오프 플래그
        bool is_onServer = false;
        //클라이언트 소켓 리스트
        public static List<Socket> clientSocketList = new List<Socket>();

        public ChatServerForm()
        {
            InitializeComponent();
            ServerStatus.Tag = "Stop";
        }

        //서버 시작 버튼을 클릭
        private void ServerOnOffBtn_Click(object sender, EventArgs e)
        {
            //서버 시작
            if (ServerStatus.Tag.ToString() == "Stop")
            {
                is_onServer = true;

                ServerStatus.Text = "Server On";
                ServerStatus.Tag = "Start";

                ServerOnOffBtn.Text = "Server Off";

                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEP = new IPEndPoint(thisAddress, port);
                mainSocket.Bind(serverEP);
                mainSocket.Listen(10);

                mainSocket.BeginAccept(AcceptCallback, null);
            }
            //서버 끊기
            else
            {
                is_onServer = false;

                ServerStatus.Text = "Server Off";
                ServerStatus.Tag = "Stop";

                ServerOnOffBtn.Text = "Server On";

                for (int i = 0; i < clientSocketList.Count; i++)
                {
                    clientSocketList[i].Disconnect(false);
                }
                clientSocketList.Clear();
                mainSocket.Close();
            }
        }

        //연결 요청 콜백 함수
        void AcceptCallback(IAsyncResult ar)
        {
            //서버가 닫혀있으면 콜백 정지
            if (!is_onServer)
            {
                if (mainSocket.Connected)
                {
                    mainSocket.EndAccept(ar);
                }
                return;
            }
            // 클라이언트의 연결 요청을 수락한다
            Socket client = mainSocket.EndAccept(ar);
            // 또 다른 클라이언트의 연결을 대기한다 (재귀함수)
            mainSocket.BeginAccept(AcceptCallback, null);

            AsyncObject obj = new AsyncObject(MAX_BYTE);
            obj.WorkingSocket = client;
            // 연결된 클라이언트 리스트에 추가해준다
            clientSocketList.Add(client);
            //접속 메시지 보내기
            string s = string.Format("클라이언트 (@ {0})가 연결되었습니다.", client.RemoteEndPoint);
            SendMessage("server", s);
            //클라이언트 데이터를 받는다
            client.BeginReceive(obj.Buffer, 0, MAX_BYTE, 0, DataReceived, obj);
        }

        //데이터 받는 콜백 함수
        void DataReceived(IAsyncResult ar)
        {
            //서버가 닫혀있으면 콜백 정지
            if (!is_onServer)
            {
                if (mainSocket.Connected)
                {
                    mainSocket.EndAccept(ar);
                }
                return;
            }
            // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            //호스트가 끊어져 있으면 정지
            if (!obj.WorkingSocket.Connected || !obj.WorkingSocket.IsBound)
            {
                return;
            }
            // 데이터 수신을 끝낸다
            int received = obj.WorkingSocket.EndReceive(ar);

            // 받은 데이터가 없으면(연결끊어짐) 끝낸다
            if (received <= 0)
            {
                obj.WorkingSocket.Close();
                return;
            }

            // 텍스트로 변환한다
            string text = Encoding.UTF8.GetString(obj.Buffer);

            // 0x01 기준으로 짜른다.
            // tokens[0] - 보낸 사람 id
            // tokens[1] - 보낸 메세지
            string[] tokens = text.Split('\x01');
            string id = tokens[0];
            string msg = tokens[1];

            //받은 메시지 보내기
            SendMessage(id, msg);

            // 데이터를 받은 후엔 다시 버퍼를 비워주고 같은 방법으로 수신을 대기한다
            obj.ClearBuffer();

            if (!obj.WorkingSocket.Connected || !obj.WorkingSocket.IsBound)
            {
                return;
            }
            //수신 대기
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, MAX_BYTE, 0, DataReceived, obj);
        }

        //서버쪽에서 모든 클라이언트에게 메시지 보내는 함수
        void SendMessage(string id, string s)
        {
            s = s.Trim();
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            byte[] data = GetBytesToSend(id, s);

            ToSendAll(data);

            SetText(id + ": " + s);
            SetText("\r\n");
        }

        //id, 텍스트를 받아서 바이트로 바꿔주는 함수
        byte[] GetBytesToSend(string id, string s)
        {
            byte[] bDts = Encoding.UTF8.GetBytes(id + '\x01' + s);
            return bDts;
        }

        //클라이언트 모두에게 바이트 데이터를 넘기는 함수
        void ToSendAll(byte[] data)
        {
            for (int i = clientSocketList.Count - 1; i >= 0; i--)
            {
                Socket socket = clientSocketList[i];
                try
                {
                    //전송
                    socket.Send(data);
                }
                catch
                {
                    try
                    {
                        //전송 취소
                        socket.Dispose();
                    }
                    catch
                    {

                    }
                    //리스트에서 삭제
                    clientSocketList.RemoveAt(i);
                }
            }
        }

        //UI 쓰레드로 ChatBox에 글을 표시하기 위해 일을 넘기는 함수
        public void SetText(string text)
        {
            if (this.ChatBox.InvokeRequired)
            {
                SetTextDelegate d = new SetTextDelegate(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.ChatBox.AppendText(text);
            }
        }

        private void ChatServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            is_onServer = false;
            for (int i = 0; i < clientSocketList.Count; i++)
            {
                clientSocketList[i].Disconnect(false);
            }
            clientSocketList.Clear();

            mainSocket.Close();
            Application.Exit();
        }
    }

    //소켓과 버퍼를 저장하는 클레스
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
}