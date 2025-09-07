'#include "windows.bi"
''unmodified original commandline, (only win32, needs windows.bi):"
'Print *GetCommandLine()
'Print Command()

'disable globbing
'' For MinGW.org and Cygwin runtimes:
Extern _CRT_glob Alias "_CRT_glob" As Long
Dim Shared _CRT_glob As Long = 0
'' For MinGW-w64 runtime:
Extern _dowildcard Alias "_dowildcard" As Long
Dim Shared _dowildcard As Long = 0


/'
Sub CPrint(Byval sText As String, Byval iColor As Integer)
    Dim As Integer C = color()
    Color iColor
    Print sText
    Color C
End Sub
'/

Sub CPrint(Byval sText As String, Byval fgColor As Integer = 7, Byval bgColor As Integer = 0)
    Dim As Integer oldColor = Color(fgColor, bgColor)
    Print sText
    'restore
    Color LoWord(oldColor), HiWord(oldColor)
End Sub

Sub CmdList(Byval s As string)
    'Dim As String TEST_COMMAND = "dir /r """ + s  + """" '+ " | find "":$DATA""
    If s <> "" Then s = " " & s
    Dim As String TEST_COMMAND = "dir" & s
    'CPrint TEST_COMMAND , 13
    Open Pipe TEST_COMMAND For Input As #1
    Dim As String ln
    Dim As String sln
    Print
    Do Until EOF(1)
        Line Input #1, ln
        sln = LCase(Trim(ln))
        'If Left(sln, 6) <> "volume" Then
         '   ln = "  " + ln
        If Instr(ln, "<DIR>") Then
            CPrint ln, 14
        ElseIf Instr(ln, "<SYMLINK") Then
            CPrint ln, 10
        ELseIf Instr(ln, ":$DATA") Then
            CPrint ln, 12
        ELseIf Left(Trim(ln), 6) = "Volume" Then
            CPrint ln, 7
        ELseIf Left(Trim(ln), 12) = "Directory of" Then
            CPrint ln, 10, 0
        ELseIf Left(ln, 3) = "   " Then
            CPrint ln, 7, 0
        ELseIf Trim(ln) = "" Then
            CPrint ln, 7, 0
        Else
            CPrint ln, 15
        End If
        'End If
    Loop
    Close #1
End Sub


CmdList command