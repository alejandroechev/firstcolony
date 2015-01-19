using System;
using System.IO;
using System.Collections;

namespace NetworkModule
{
	/// <summary>
	/// Clase encarga de logear informacion 
	/// </summary>
	class NetworkLogger
	{
		#region Members

		/// <summary>
		/// Atributos
		/// </summary>
 		private int	m_nNMessages;
		private string m_sLocalPath;
		public string LocalPath { set { m_sLocalPath = value; } }
        private bool m_bLogFirst = true;
		private string m_sLogFilename = "network.txt";
        private string m_sInfoHeader = "INFO";
        private string m_sErrorHeader = "ERROR";

        /// <summary>
        /// Singleton de la clase
        /// </summary>
        private static NetworkLogger Logger = null;
        public static NetworkLogger getInstance()
        {
            if (Logger == null)
                Logger = new NetworkLogger();
            return Logger;
        }

        #endregion

        /// <summary>
        /// Constructor privado para Singleton
        /// </summary>
        private NetworkLogger()
        {
            m_sLocalPath = NetworkUtils.getCurrentDirectory();
        }
		  
        /// <summary>
        /// Agrega al log el mensaje
        /// </summary>
        /// <param name="sMsg">Mensaje a loggear</param>
		public void info(string sMsg)
		{
			try
			{
				// Usamos lock para evitar varias thread al mismo tiempo
				lock(typeof(NetworkLogger))
				{
					m_nNMessages++;

					// Abrimos streams para archivo
					System.IO.FileStream fstrLogFile;
					if(m_bLogFirst)
					{
						if (File.Exists(Path.Combine(m_sLocalPath, m_sLogFilename)))
							File.Delete(Path.Combine(m_sLocalPath, m_sLogFilename));
						fstrLogFile = new FileStream(Path.Combine(m_sLocalPath, m_sLogFilename), FileMode.Create);
						m_bLogFirst = false;
					}
					else
						fstrLogFile = new FileStream(Path.Combine(m_sLocalPath, m_sLogFilename), FileMode.Append);
					System.IO.StreamWriter strWriter = new StreamWriter(fstrLogFile); 

					// Guardamos en archivo
                    strWriter.WriteLine("" + m_nNMessages + ": ["+m_sInfoHeader+"] " + sMsg);
					//strWriter.Flush();
			
					// Cerramos streams
					strWriter.Close();
					fstrLogFile.Close();				
				}
			}
			catch(Exception exc)
			{
				//Ignoramos excepcion
			}
		}
                
	
        /// <summary>
        /// Agrega un mensaje de error a archivo
        /// </summary>
        /// <param name="sMsg">Mensaje de error</param>
        public void error(string sMsg)
		{
			try
			{
				// Usamos lock para evitar varias thread al mismo tiempo
				lock(typeof(NetworkLogger))
				{
					m_nNMessages++;

					// Abrimos streams para archivo
					System.IO.FileStream fstrLogFile;
					if(m_bLogFirst)
					{
                        if (File.Exists(Path.Combine(m_sLocalPath, m_sLogFilename)))
                            File.Delete(Path.Combine(m_sLocalPath, m_sLogFilename));
                        fstrLogFile = new FileStream(Path.Combine(m_sLocalPath, m_sLogFilename), FileMode.Create);
						m_bLogFirst = false;
					}
					else
                        fstrLogFile = new FileStream(Path.Combine(m_sLocalPath, m_sLogFilename), FileMode.Append);
					System.IO.StreamWriter strWriter = new StreamWriter(fstrLogFile); 

					// Guardamos en archivo
                    strWriter.WriteLine("" + m_nNMessages + ": [" + m_sErrorHeader + "] " + sMsg);
					//strWriter.Flush();
			
					// Cerramos streams
					strWriter.Close();
					fstrLogFile.Close();

				
				}
			}
			catch(Exception exc)
			{
				//Ignoramos excepcion
			}
		}

		
	
                
	}
}
