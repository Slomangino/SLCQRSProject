﻿using Autofac;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrbanSpork.Domain.Interfaces.ReadModel;
using UrbanSpork.Domain.Interfaces.WriteModel;
using UrbanSpork.DataAccess.DataAccess;
using AutoMapper;
using UrbanSpork.DataAccess.DataTransferObjects;
using UrbanSpork.DataAccess.Repositories;
using UrbanSpork.DataAccess.WriteModel;
using UrbanSpork.DataAccess.ReadModel.QueryCommands;
using UrbanSpork.DataAccess.ReadModel.QueryHandlers;

namespace UrbanSpork.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            //this is the builder object
            var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            
            this.Configuration = builder.Build();

            Mapper.Initialize(cfg => {
                cfg.CreateMap<Users, UserDTO>();
                cfg.CreateMap<UserDTO, Users>();
                // cfg.CreateMap<Bar, BarDto>();
            });
        }

        public IConfiguration Configuration { get; private set; }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the collection. Don't build or return
            // any IServiceProvider or the ConfigureContainer method
            // won't get called.
            //services.AddAutofac();
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddEntityFrameworkNpgsql().AddDbContext<UrbanDbContext>(options => options.UseNpgsql(connectionString, m => m.MigrationsAssembly("UrbanSpork.DataAccess")));

            services.AddMvc();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Utility
            builder.RegisterType<UserRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<QueryProcessor>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<CommandDispatcher>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<UserDTO>().AsImplementedInterfaces().InstancePerLifetimeScope();

            //Commands
            builder.RegisterType<CreateSingleUserCommand>().AsImplementedInterfaces().InstancePerLifetimeScope();

            //Command Handlers
            builder.RegisterType<CreateSingleUserCommandHandler>().AsImplementedInterfaces().InstancePerLifetimeScope();

            //Query
            builder.RegisterType<GetAllUsersQuery>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<GetUserByIdQuery>().AsImplementedInterfaces().InstancePerLifetimeScope();

            //Query Handlers
            builder.RegisterType<GetUserByIdQueryHandler>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<GetAllUsersQueryHandler>().AsImplementedInterfaces().InstancePerLifetimeScope();

        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
        }

    }
}
