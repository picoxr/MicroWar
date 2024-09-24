using System;
using System.Collections.Generic;
using System.IO;

namespace Pico
{
	namespace Avatar
	{
		namespace IO
		{
			public class JSCompress
			{
				// From: https://github.com/douglascrockford/JSMin
				private const int EOF = -1;
				private int the_a;
				private int the_b;
				private int look_ahead = EOF;
				private int the_x = EOF;
				private int the_y = EOF;
				private BinaryReader mReader = null; // stream to process the file byte by byte
				private string mOriginalData = ""; // data from original file
				private string mModifiedData = ""; // processed data
				private bool mIsError = false; // becomes true if any error happens
				private string mErr = ""; // error message 
				private List<string> mFileList = new List<string>();

				/// <summary>
				/// Constructor - does all the processing
				/// </summary>
				/// <param name="f">file path</param>
				public JSCompress(string dir)
				{
					try
					{
						listDirectory(dir, 0);
						foreach (string js_file in mFileList)
						{
							if (File.Exists(js_file))
							{
								reset();
								//read contents completely. This is only for test purposes. The actual processing is done by another stream
								StreamReader rdr = new StreamReader(js_file);
								mOriginalData = rdr.ReadToEnd();
								rdr.Close();

								mReader = new BinaryReader(new FileStream(js_file, FileMode.Open));
								jsmin();
								mReader.Close();

								//write modified data
								StreamWriter wrt = new StreamWriter(js_file);
								wrt.Write(mModifiedData);
								wrt.Close();
							}
							else
							{
								mIsError = true;
								mErr = "File does not exist";
							}
						}
					}
					catch (Exception ex)
					{
						mIsError = true;
						mErr = ex.Message;
					}
				}

				private void reset()
				{
					look_ahead = EOF;
					the_x = EOF;
					the_y = EOF;
					mOriginalData = "";
					mModifiedData = "";
				}

				/// <summary>
				/// Check if a byte is alphanumeric
				/// </summary>
				/// <param name="c">byte to check</param>
				/// <returns>retval - 1 if yes. else 0</returns>
				private bool is_alphanum(int c)
				{
					return ((c >= 'a' && c <= 'z') ||
					        (c >= '0' && c <= '9') ||
					        (c >= 'A' && c <= 'Z') ||
					        c == '_' || c == '$' || c == '\\' || c > 126);
				}

				private int get()
				{
					int codeunit = look_ahead;
					look_ahead = EOF;
					if (codeunit == EOF)
					{
						bool endProcess = (mReader.PeekChar() == -1);
						if (!endProcess)
						{
							codeunit = mReader.ReadByte();
						}
						else
						{
							codeunit = EOF;
						}
					}

					if (codeunit >= ' ' || codeunit == '\n' || codeunit == EOF)
					{
						return codeunit;
					}

					if (codeunit == '\r')
					{
						return '\n';
					}

					return ' ';
				}

				// peek -- get the next character without advancing
				private int peek()
				{
					look_ahead = get();
					return look_ahead;
				}

				// next -- get the next character, excluding comments. peek() is used to see if a '/' is followed by a '/' or '*'.
				private int next()
				{
					int codeunit = get();
					if (codeunit == '/')
					{
						switch (peek())
						{
							case '/':
								for (;;)
								{
									codeunit = get();
									if (codeunit <= '\n')
									{
										break;
									}
								}

								break;
							case '*':
								get();
								while (codeunit != ' ')
								{
									switch (get())
									{
										case '*':
											if (peek() == '/')
											{
												get();
												codeunit = ' ';
											}

											break;
										case EOF:
											throw new Exception("Unterminated comment.");
									}
								}

								break;
						}
					}

					the_y = the_x;
					the_x = codeunit;
					return codeunit;
				}

				//action -- do something! What you do is determined by the argument:
				// 1   Output A. Copy B to A. Get the next B.
				// 2   Copy B to A. Get the next B. (Delete A).
				// 3   Get the next B. (Delete B).
				//     action treats a string as a single character.
				//     action recognizes a regular expression if it is preceded by the likes of
				//     '(' or ',' or '='.
				private void action(int determined)
				{
					switch (determined)
					{
						case 1:
							addToOutput(the_a);
							if (
								(the_y == '\n' || the_y == ' ')
								&& (the_a == '+' || the_a == '-' || the_a == '*' || the_a == '/')
								&& (the_b == '+' || the_b == '-' || the_b == '*' || the_b == '/')
							)
							{
								addToOutput(the_y);
							}

							goto case 2;
						case 2:
							the_a = the_b;
							if (the_a == '\'' || the_a == '"' || the_a == '`')
							{
								while (true)
								{
									addToOutput(the_a);
									the_a = get();
									if (the_a == the_b)
									{
										break;
									}

									if (the_a == '\\')
									{
										addToOutput(the_a);
										the_a = get();
									}

									if (the_a == EOF)
									{
										throw new Exception("Unterminated string literal.");
									}
								}
							}

							goto case 3;
						case 3:
							the_b = next();
							if (the_b == '/' && (
								    the_a == '(' || the_a == ',' || the_a == '=' || the_a == ':'
								    || the_a == '[' || the_a == '!' || the_a == '&' || the_a == '|'
								    || the_a == '?' || the_a == '+' || the_a == '-' || the_a == '~'
								    || the_a == '*' || the_a == '/' || the_a == '{' || the_a == '}'
								    || the_a == ';'
							    ))
							{
								addToOutput(the_a);
								if (the_a == '/' || the_a == '*')
								{
									addToOutput(' ');
								}

								addToOutput(the_b);
								while (true)
								{
									the_a = get();
									if (the_a == '[')
									{
										while (true)
										{
											addToOutput(the_a);
											the_a = get();
											if (the_a == ']')
											{
												break;
											}

											if (the_a == '\\')
											{
												addToOutput(the_a);
												the_a = get();
											}

											if (the_a == EOF)
											{
												throw new Exception(
													"Unterminated set in Regular Expression literal."
												);
											}
										}
									}
									else if (the_a == '/')
									{
										switch (peek())
										{
											case '/':
											case '*':
												throw new Exception("Unterminated set in Regular Expression literal.");
										}

										break;
									}
									else if (the_a == '\\')
									{
										addToOutput(the_a);
										the_a = get();
									}

									if (the_a == EOF)
									{
										throw new Exception("Unterminated Regular Expression literal.");
									}

									addToOutput(the_a);
								}

								the_b = next();
							}

							break;
					}
				}

				//jsmin -- Copy the input to the output, deleting the characters which are
				// insignificant to JavaScript. Comments will be removed. Tabs will be
				// replaced with spaces. Carriage returns will be replaced with linefeeds.
				// Most spaces and linefeeds will be removed.
				private void jsmin()
				{
					if (peek() == 0xEF)
					{
						get();
						get();
						get();
					}

					the_a = '\n';
					action(3);
					while (the_a != EOF)
					{
						switch (the_a)
						{
							case ' ':
								action(
									is_alphanum(the_b) ? 1 : 2
								);
								break;
							case '\n':
								switch (the_b)
								{
									case '{':
									case '[':
									case '(':
									case '+':
									case '-':
									case '!':
									case '~':
										action(1);
										break;
									case ' ':
										action(3);
										break;
									default:
										action(
											is_alphanum(the_b)
												? 1
												: 2
										);
										break;
								}

								break;
							default:
								switch (the_b)
								{
									case ' ':
										action(
											is_alphanum(the_a)
												? 1
												: 3
										);
										break;
									case '\n':
										switch (the_a)
										{
											case '}':
											case ']':
											case ')':
											case '+':
											case '-':
											case '"':
											case '\'':
											case '`':
												action(1);
												break;
											default:
												action(
													is_alphanum(the_a)
														? 1
														: 3
												);
												break;
										}

										break;
									default:
										action(1);
										break;
								}

								break;
						}
					}
				}

				/// <summary>
				/// Add character to modified data string
				/// </summary>
				/// <param name="c">char to add</param>
				private void addToOutput(int c)
				{
					mModifiedData += (char)c;
				}

				/// <summary>
				/// 列出path路径对应的文件夹中的子文件夹和文件
				/// </summary>
				/// <param name="path">需要列出内容的文件夹的路径</param>
				/// <param name="leval">当前递归层级，用于控制输出前导空格的数量</param>
				private void listDirectory(string path, int leval)
				{
					DirectoryInfo theFolder = new DirectoryInfo(@path);

					leval++;

					//遍历文件
					foreach (FileInfo NextFile in theFolder.GetFiles())
					{
						string nextFileName = NextFile.FullName;
						if (nextFileName.EndsWith(".js"))
						{
							mFileList.Add(nextFileName);
						}
					}

					//遍历文件夹
					foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
					{
						listDirectory(NextFolder.FullName, leval);
					}
				}
			}
		}
	}
}