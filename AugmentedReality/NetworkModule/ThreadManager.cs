using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;

namespace NetworkModule
{
    /// <summary>
    /// Clase que maneja todos los threads de red existentes
    /// </summary>
    public class ThreadManager
    {
        /// <summary>
        /// Tabla de threads creados
        /// </summary>
        private Dictionary<string, Thread> m_htThreads;
        
        /// <summary>
        /// Manejo de singleton de la clase
        /// </summary>
        private static ThreadManager Manager = null;
        public static ThreadManager getInstance()
        {
            if (Manager == null)
                Manager = new ThreadManager();

            return Manager;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private ThreadManager()
        {
            m_htThreads = new Dictionary<string, Thread>();
            
        }

        /// <summary>
        /// Agrega un nuevo thread a la lista
        /// </summary>
        /// <param name="th">Thread</param>
        public bool addThread(string sName, Thread th)
        {
            if (m_htThreads.ContainsKey(sName))
                return false;
            
            m_htThreads.Add(sName,th);
            return true;
        }

       

        /// <summary>
        /// Termina todos los threads almacenados
        /// </summary>
        public void abortAll()
        {
            foreach (KeyValuePair<string,Thread> de in m_htThreads)
            {
                Thread t = de.Value;
                if (t != null)
                {
                    t.Abort();
                }
            }

        }

        
    }
}
