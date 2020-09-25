﻿var React = require("react")
var {Link} = require("react-router")
var {connect} = require("react-redux")
var {Menus} = require("../consts")
var ContentHeader = require('../components/contentheader')


class Menu extends React.Component{
    render() {
        let menu = this.props.menu;
        if(menu.hide){
            return null;
        }

        let className = this.props.menu.selected ? "active" : ""
        let icon = null;
        if (this.props.menu.icon) {
            icon = <i className={this.props.menu.icon}></i>
        }
        else{
            icon = <i className="fa fa-circle-o"></i>
        }
        let children = null;
        if (this.props.menu.children && this.props.menu.children.length > 0) {
            className = className + " treeview";
            children = (
                <ul className="treeview-menu">
                    {
                        this.props.menu.children.map(menu => {
                            return <Menu menu={menu} key={menu.key}/>
                        })
                    }
                </ul>
                )
        }

        return (
            <li className={className}>
                <Link to={this.props.menu.url}>
                    {icon}
                    <span>{this.props.menu.text}</span>
                </Link>

                {children}
            </li>
            )
    }
}

class App extends React.Component{
    constructor() {
        super();

        this.menus = Menus
    }

    render() {
        this.menus.forEach(m => {
            m.selected = this.props.menu == m.key;
            if (m.children) {
                m.children.forEach(sm => {
                    sm.selected = this.props.subMenu == sm.key
                });
            }
        });

        return (
            <div className="wrapper">
                <header className="main-header">
                    <a href="/" className="logo">
                        {"DotNetBlog".L()}
                    </a>

                    <nav className="navbar navbar-static-top" role="navigation">

                        <a href="javascript:void(0)" className="sidebar-toggle" data-toggle="offcanvas" role="button">
                            <span className="sr-only">Toggle navigation</span>
                        </a>
                        <div className="navbar-custom-menu">
                            <ul className="nav navbar-nav">
                                <li><a href="/Identity/Account/Manage/ChangePassword">{"changePassword".L()}</a></li>
                                <li>
                                    <a href="/Identity/Account/Logout">
                                        <i className="fa fa-exit"></i>
                                        {"logoff".L()}
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </nav>

                </header>

                <aside className="main-sidebar">
                    <div className="sidebar">
                        <div className="user-panel">
                            <div className="pull-left image">
                                <img src="https://almsaeedstudio.com/themes/AdminLTE/dist/img/user2-160x160.jpg" className="img-circle" alt={"userImage".L()} />
                            </div>
                            <div className="pull-left info">
                                <p>
                                    <b>{user.nickname}</b>
                                </p>

                                <span>
                                    <i className="fa fa-circle text-success"></i>
                                    {"greetingUser".L(user.username)}
                                </span>
                            </div>
                        </div>

                        <ul className="sidebar-menu">
                            <li className="header">{"siteNavigation".L()}</li>
                            {
                                this.menus.map(menu=>{
                                    return <Menu menu={menu} key={menu.key}/>
                                })
                            }
                        </ul>
                    </div>                    
                </aside>

                <div className="content-wrapper" style={contentWrapperStyle}>
                    <ContentHeader></ContentHeader>

                    {this.props.children}
                </div>

                <footer className="main-footer">
                    <div className="pull-right hidden-xs">
                        <b>Version</b> 2.3.3
                    </div>
                    <strong>Copyright © 2014-2015 <a href="http://almsaeedstudio.com">Almsaeed Studio</a>.</strong> All rights
                    reserved.
                </footer>
            </div>
        )
    }
}

const contentWrapperStyle = {
    "minHeight": "724px"
}

function mapStateToProps(state) {
    return {
        menu: state.blog.menu,
        subMenu: state.blog.subMenu
    }
}

module.exports = connect(mapStateToProps)(App)