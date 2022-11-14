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
        //UI �����忡�� ���� �ѱ������ �븮��
        delegate void SetTextDelegate(string s);
        Socket mainSocket;
        //����ȣ��Ʈ �ּ�, ���� ��Ʈ��ȣ ���
        IPAddress thisAddress = IPAddress.Loopback;
        int port = 2022;
        int MAX_BYTE = 4096;
        //���� �� ���� �÷���
        bool is_onServer = false;
        //Ŭ���̾�Ʈ ���� ����Ʈ
        public static List<Socket> clientSocketList = new List<Socket>();

        public ChatServerForm()
        {
            InitializeComponent();
            ServerStatus.Tag = "Stop";
        }

        //���� ���� ��ư�� Ŭ��
        private void ServerOnOffBtn_Click(object sender, EventArgs e)
        {
            //���� ����
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
            //���� ����
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

        //���� ��û �ݹ� �Լ�
        void AcceptCallback(IAsyncResult ar)
        {
            //������ ���������� �ݹ� ����
            if (!is_onServer)
            {
                if (mainSocket.Connected)
                {
                    mainSocket.EndAccept(ar);
                }
                return;
            }
            // Ŭ���̾�Ʈ�� ���� ��û�� �����Ѵ�
            Socket client = mainSocket.EndAccept(ar);
            // �� �ٸ� Ŭ���̾�Ʈ�� ������ ����Ѵ� (����Լ�)
            mainSocket.BeginAccept(AcceptCallback, null);

            AsyncObject obj = new AsyncObject(MAX_BYTE);
            obj.WorkingSocket = client;
            // ����� Ŭ���̾�Ʈ ����Ʈ�� �߰����ش�
            clientSocketList.Add(client);
            //���� �޽��� ������
            string s = string.Format("Ŭ���̾�Ʈ (@ {0})�� ����Ǿ����ϴ�.", client.RemoteEndPoint);
            SendMessage("server", s);
            //Ŭ���̾�Ʈ �����͸� �޴´�
            client.BeginReceive(obj.Buffer, 0, MAX_BYTE, 0, DataReceived, obj);
        }

        //������ �޴� �ݹ� �Լ�
        void DataReceived(IAsyncResult ar)
        {
            //������ ���������� �ݹ� ����
            if (!is_onServer)
            {
                if (mainSocket.Connected)
                {
                    mainSocket.EndAccept(ar);
                }
                return;
            }
            // BeginReceive���� �߰������� �Ѿ�� �����͸� AsyncObject �������� ��ȯ�Ѵ�
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            //ȣ��Ʈ�� ������ ������ ����
            if (!obj.WorkingSocket.Connected || !obj.WorkingSocket.IsBound)
            {
                return;
            }
            // ������ ������ ������
            int received = obj.WorkingSocket.EndReceive(ar);

            // ���� �����Ͱ� ������(���������) ������
            if (received <= 0)
            {
                obj.WorkingSocket.Close();
                return;
            }

            // �ؽ�Ʈ�� ��ȯ�Ѵ�
            string text = Encoding.UTF8.GetString(obj.Buffer);

            // 0x01 �������� ¥����.
            // tokens[0] - ���� ��� id
            // tokens[1] - ���� �޼���
            string[] tokens = text.Split('\x01');
            string id = tokens[0];
            string msg = tokens[1];

            //���� �޽��� ������
            SendMessage(id, msg);

            // �����͸� ���� �Ŀ� �ٽ� ���۸� ����ְ� ���� ������� ������ ����Ѵ�
            obj.ClearBuffer();

            if (!obj.WorkingSocket.Connected || !obj.WorkingSocket.IsBound)
            {
                return;
            }
            //���� ���
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, MAX_BYTE, 0, DataReceived, obj);
        }

        //�����ʿ��� ��� Ŭ���̾�Ʈ���� �޽��� ������ �Լ�
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

        //id, �ؽ�Ʈ�� �޾Ƽ� ����Ʈ�� �ٲ��ִ� �Լ�
        byte[] GetBytesToSend(string id, string s)
        {
            byte[] bDts = Encoding.UTF8.GetBytes(id + '\x01' + s);
            return bDts;
        }

        //Ŭ���̾�Ʈ ��ο��� ����Ʈ �����͸� �ѱ�� �Լ�
        void ToSendAll(byte[] data)
        {
            for (int i = clientSocketList.Count - 1; i >= 0; i--)
            {
                Socket socket = clientSocketList[i];
                try
                {
                    //����
                    socket.Send(data);
                }
                catch
                {
                    try
                    {
                        //���� ���
                        socket.Dispose();
                    }
                    catch
                    {

                    }
                    //����Ʈ���� ����
                    clientSocketList.RemoveAt(i);
                }
            }
        }

        //UI ������� ChatBox�� ���� ǥ���ϱ� ���� ���� �ѱ�� �Լ�
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

    //���ϰ� ���۸� �����ϴ� Ŭ����
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