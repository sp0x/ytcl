Imports CommandLine

Public Class Options
    <[Option]("f", "format", DefaultValue:="mp3", Required:=False, HelpText:="The format of the audio.")> _
    Public Property Format As String

    <OptionAttribute("q", "quality", Required:=False, HelpText:="The quality of the video.")> _
    Public Property Quality As String

    <OptionAttribute("i", "info", DefaultValue:=False, Required:=False)>
    Public Property IsInformationRequest As Boolean

    <OptionAttribute("v", "video", DefaultValue:=False, Required:=False)>
    Public Property OnlyVideo As Boolean

    <ValueOption(0)> _
    Public Property Link As String

    <ValueOption(1)> _
    Public Property OutputPath As String

End Class

