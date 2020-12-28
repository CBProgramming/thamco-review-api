using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReviewData;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ReviewRepository;

namespace ReviewService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication()
                .AddJwtBearer("CustomerAuth", options =>
                {
                    options.Authority = Configuration.GetValue<string>("CustomerAuthServerUrl");
                    options.Audience = "customer_ordering_api";
                })
                .AddJwtBearer("StaffAuth", options =>
                {
                    options.Authority = Configuration.GetValue<string>("StaffAuthServerUrl");
                    options.Audience = "customer_ordering_api";
                });

            services.AddAuthorization(OptionsBuilderConfigurationExtensions =>
            {
                OptionsBuilderConfigurationExtensions.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("CustomerAuth")
                .Build();

                OptionsBuilderConfigurationExtensions.AddPolicy("OrderingAPIOnly", policy =>
                policy.AddAuthenticationSchemes("CustomerAuth")
                .RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "client_id" && c.Value == "customer_ordering_api"))
                .Build());

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOnly", policy =>
                policy.AddAuthenticationSchemes("CustomerAuth")
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "client_id" && c.Value == "customer_web_app"))
                .Build());

                OptionsBuilderConfigurationExtensions.AddPolicy("StaffOnly", policy =>
                policy.AddAuthenticationSchemes("StaffAuth")
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "client_id" && c.Value == "customer_management_web_app"))
                .Build());
            });

                services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<ReviewDb>(options =>
            {
                var cs = Configuration.GetConnectionString("ReviewConnection");
                options.UseSqlServer(cs);
            });
            services.AddScoped<IReviewRepository, ReviewRepository.ReviewRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
