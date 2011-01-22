using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

namespace HttpDebugger {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }

    private TcpListener myListener;
    private int port = 3000;
    delegate void SetTextCallback(string text);
    delegate void AppendTextCallback(string text);

    private void Form1_Load(object sender, EventArgs e) {
      try {
        //start listing on the given port
        myListener = new TcpListener(port);
        myListener.Start();
        Console.WriteLine("Web Server Running... Press ^C to Stop...");

        //start the thread which calls the method 'StartListen'
        Thread th = new Thread(new ThreadStart(StartListen));
        th.Start();

      } catch (Exception ex) {
        Console.WriteLine("An Exception Occurred while Listening :" + ex.ToString());
      }
    }

    private void SetText(string text) {
      // InvokeRequired required compares the thread ID of the
      // calling thread to the thread ID of the creating thread.
      // If these threads are different, it returns true.
      if (this.textBox1.InvokeRequired) {
        SetTextCallback d = new SetTextCallback(SetText);
        this.Invoke(d, new object[] { text });
      } else {
        this.textBox1.Text = text;
      }
    }

    private void AppendText(string text) {
      // InvokeRequired required compares the thread ID of the
      // calling thread to the thread ID of the creating thread.
      // If these threads are different, it returns true.
      if (this.textBox1.InvokeRequired) {
        var d = new AppendTextCallback(AppendText);
        this.Invoke(d, new object[] { text });
      } else {
        this.textBox1.Text += text;
      }
    }


    public void StartListen() {

      int iStartPos = 0;
      String sRequest;
      String sDirName;
      String sRequestedFile;
      String sErrorMessage;
      String sLocalDir;
      String sMyWebServerRoot = "C:\\MyWebServerRoot\\";
      String sPhysicalFilePath = "";
      String sFormattedMessage = "";
      String sResponse = "";


      while (true) {
        //Accept a new connection
        Socket mySocket = myListener.AcceptSocket();

        Console.WriteLine("Socket Type " + mySocket.SocketType);
        if (mySocket.Connected) {
          Console.WriteLine("\nClient Connected!!\n==================\nCLient IP {0}\n", mySocket.RemoteEndPoint);


          //make a byte array and receive data from the client 
          Byte[] bReceive = new Byte[4096];
          int i = mySocket.Receive(bReceive, bReceive.Length, 0);


          //Convert Byte to String
          string sBuffer = Encoding.ASCII.GetString(bReceive);

          SetText(sBuffer);

          var method = sBuffer.Split(' ')[0];
          if (method == "POST") {            
            var contentLength = int.Parse(Regex.Match(sBuffer, "Content-Length: (\\d+)").Groups[1].Value);

            bReceive = new Byte[contentLength];
            i = mySocket.Receive(bReceive, bReceive.Length, 0);
            sBuffer = Encoding.ASCII.GetString(bReceive);
            AppendText(sBuffer);
          }

          mySocket.Close();

          continue;
        }
      }
    }
  }
}
