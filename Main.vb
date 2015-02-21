Imports ytDownloader
Imports ytDownloader.Extraction
Imports CommandLine

Public Module Main
    Public Wl As Action(Of String) = AddressOf Console.WriteLine
    Public Wr As Action(Of String) = AddressOf Console.Write
    Private _updateDelta As Double = 0
    Public UpdateInterval As Double = 2D
    Public Timer As New Stopwatch

    Private Sub updateHandler(sender As Object, e As ProgressEventArgs)
        Dim tsAction As Action = _
            Sub()
                Dim str As String = Nothing
                Dim shouldPrint As Boolean = False
                Dim delta As Double = Math.Abs(e.ProgressPercentage - _updateDelta)
                If _updateDelta.Equals(0D) Or _updateDelta.Equals(100D) Then
                    shouldPrint = True
                    _updateDelta = e.ProgressPercentage
                ElseIf delta > UpdateInterval Then ' get delta 
                    shouldPrint = True
                    _updateDelta = e.ProgressPercentage
                End If

                If shouldPrint Then
                    Dim eta As TimeSpan = Timer.Elapsed
                    Wr(String.Format("[{0}:{1}] ", eta.Minutes, eta.Seconds))

                    str = String.Format("{0}: ", e.Flag.ToString)
                    Console.ForegroundColor = ConsoleColor.Green
                    Wr(str)
                    str = String.Format("{0:F2}", e.ProgressPercentage)
                    Console.ForegroundColor = ConsoleColor.Red
                    Wl(str)
                    Console.ForegroundColor = ConsoleColor.White
                End If
            End Sub
        Task.Factory.StartNew(tsAction)
    End Sub

    Private Sub DownloadFinished(sender As Object, e As IoFinishedEventArgs)
        Timer.Stop()
        Select Case e.Mode
            Case IoMode.File
                Console.ForegroundColor = ConsoleColor.Green
                Wl(String.Format("Finished [in {0}] downloading and saved to {1}", Timer.Elapsed, e.Path))
                Console.ForegroundColor = ConsoleColor.White
            Case IoMode.Streaming

        End Select
    End Sub

    Private Sub PrintStatement(a As String, b As String)
        Dim str As String = String.Format("{0}: ", a)
        Console.ForegroundColor = ConsoleColor.Green
        Wr(str)
        str = String.Format("{0}", b)
        Console.ForegroundColor = ConsoleColor.Red
        Wl(str)
        Console.ForegroundColor = ConsoleColor.White
    End Sub
    
    Public Sub HandleArguments(ops As Options, Optional startEach As Boolean = False)
        Dim executor As Action(Of Downloader) = Nothing
        If startEach Then executor = AddressOf LaunchDownload
        Downloader.Factory.TryParseQuality(ops.Quality)
        If Not ops.IsInformationRequest Then
            Downloader.Factory.ExecuteList(ops.Link, ops.OutputPath, ops.OnlyVideo, ops.Format, ops.Quality, _
                                      executor)
        Else
            'TODO: Implement info fetching.
            Dim dOps As DownloadOptions = DownloadOptionsBuilder.Build(ops.OutputPath, False, "", 0)
            Dim codecs As IEnumerable(Of VideoCodecInfo) = dOps.GetCodecs(ops.Link)
            codecs = codecs.OrderBy(Function(x) x.FormatCode).ThenBy(Function(x) x.Resolution)
            For Each codec As VideoCodecInfo In codecs
                Wl(codec.ToString)
            Next
        End If
    End Sub

    Public Sub LaunchDownload(dldr As Downloader)
    Try
        dldr = Downloader.Initialize(dldr)
        If TypeOf dldr Is AudioDownloader Then
            PrintStatement("Downloading audio", dldr.InputUrl)
        Else
            PrintStatement("Downloading video", dldr.InputUrl)
        End If
        AddHandler dldr.ExtractionProgressChanged, AddressOf updateHandler
        AddHandler dldr.DownloadProgressChanged, AddressOf updateHandler
        AddHandler dldr.DownloadFinished, AddressOf DownloadFinished
        PrintStatement("To", dldr.OutputPath)
        dldr.Start()
        DownloadCounter += 1
        catch ex as Exception
            Wl(String.format("Exception: {0}", ex.message))
        end try 
    End Sub





    Public DownloadCounter As UInt64 = 0
    Public Sub Main(args As String())
        Dim ops As Options = New Options

        Const startEach As Boolean = True
        Try
            If CommandLine.Parser.Default.ParseArguments(args, ops) Then
                If String.IsNullOrEmpty(ops.Link) Then Return
                Timer.Start()
                Try
                    HandleArguments(ops, startEach)
                    PrintStatement("Downloaded a total of", String.Format("{0} videos!", DownloadCounter))
                Catch ex As Exception
                    Wl(ex.Message)
                End Try
            Else
                Wl("Usage:")
                Wl("yoump3 http://youtubeUrl <-q [quality]> <-f [format:mp3,aac,flv,mp4,webm,3gp]>  <outputFile> <-*audio|video> <-i|info>")
            End If
        Catch ex As Exception
            Wl(ex.Message)
        End Try
        Wl("Press any key to exit:") : Console.ReadLine()
    End Sub

  


End Module
