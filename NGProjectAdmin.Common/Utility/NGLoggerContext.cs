﻿
using log4net;
using log4net.Config;
using System;
using System.IO;

namespace NGProjectAdmin.Common.Utility
{
    public class NGLoggerContext
    {
        /// <summary>
        /// ILog实例
        /// </summary>
        private static ILog logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        static NGLoggerContext()
        {
            //判断审计日志记录开关是否开启
            if (logger == null && Global.NGAdminGlobalContext.LogConfig.IsEnabled)
            {
                var repository = LogManager.CreateRepository("NETCoreRepository");
                var path = Directory.GetCurrentDirectory() + "/Log4net/log4net.config";
                //读取配置信息
                XmlConfigurator.Configure(repository, new FileInfo(path));
                logger = LogManager.GetLogger(repository.Name, "InfoLogger");
            }
        }

        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message">摘要</param>
        /// <param name="exception">异常</param>
        public static void Info(string message, Exception exception = null)
        {
            //判断审计日志记录开关是否开启
            if (logger != null && Global.NGAdminGlobalContext.LogConfig.IsEnabled)
            {
                Console.WriteLine("RuYiAdmin Runtime Info:" + message);
                if (exception == null)
                {
                    logger.Info(message);
                }
                else
                {
                    logger.Info(message, exception);
                }
            }
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">摘要</param>
        /// <param name="exception">异常</param>
        public static void Warn(string message, Exception exception = null)
        {
            //判断审计日志记录开关是否开启
            if (logger != null && Global.NGAdminGlobalContext.LogConfig.IsEnabled)
            {
                Console.WriteLine("RuYiAdmin Warnning Info:" + message);
                if (exception == null)
                {
                    logger.Warn(message);
                }
                else
                {
                    logger.Warn(message, exception);
                }
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">摘要</param>
        /// <param name="exception">异常</param>
        public static void Error(string message, Exception exception = null)
        {
            //判断审计日志记录开关是否开启
            if (logger != null && Global.NGAdminGlobalContext.LogConfig.IsEnabled)
            {
                Console.WriteLine("RuYiAdmin Error Info:" + message);
                if (exception == null)
                {
                    logger.Error(message);
                }
                else
                {
                    logger.Error(message, exception);
                }
            }
        }

        /// <summary>
        /// Debug日志
        /// </summary>
        /// <param name="message">摘要</param>
        /// <param name="exception">异常</param>
        public static void Debug(string message, Exception exception = null)
        {
            //判断审计日志记录开关是否开启
            if (logger != null && Global.NGAdminGlobalContext.LogConfig.IsEnabled)
            {
                Console.WriteLine("RuYiAdmin Debug Info:" + message);
                if (exception == null)
                {
                    logger.Debug(message);
                }
                else
                {
                    logger.Debug(message, exception);
                }
            }
        }
    }
}
