﻿
var React = require("react")
var Parallel = require("async/parallel")
var {Spinner, Bootstrap: {FormGroup}} = require("../../components")
var {Api, Dialog} = require("../../services")
var {reduxForm} = require("redux-form")

import Localization from "../../resources"

const validate = values=>{
    const errors = {}

    if(!values.host){
        errors.host = "enterBlogAddress".L();
    }

    if(!values.title){
        errors.title = "pleaseEnterTitle".L();
    }

    let topicsPerPage = parseInt(values.topicsPerPage);
    if(isNaN(Number(values.topicsPerPage)) || topicsPerPage < 1){
        errors.topicsPerPage = "enterCorrectNumberOfArticles".L();
    }

    return errors
}

class BasicConfigForm extends React.Component{

    constructor() {
        super()

        this.languageOptions = Localization.getAvailableLanguages();
    }

    render() {
        const { fields: { host, title, description, language, theme, topicsPerPage, onlyShowSummary, registration }, handleSubmit, themes } = this.props
        return (
            <form noValidate onSubmit={handleSubmit}>
                <FormGroup label={"blogAddress".L()} validation={host}>
                    <input type="text" className="form-control" {...host}/>
                </FormGroup>
                <FormGroup label={"title".L()} validation={title}>
                    <input type="text" className="form-control" {...title}/>
                </FormGroup>
                <FormGroup label={"description".L()}>
                    <input type="text" className="form-control" {...description}/>
                </FormGroup>
                <FormGroup label={"numberOfArticlesPerPage".L()} validation={topicsPerPage}>
                    <input type="text" className="form-control" {...topicsPerPage}/>
                </FormGroup>
                <FormGroup label={"selectedLanguage".L()}>
                    <select className="form-control" {...language}>
                        {this.languageOptions.map(item => {
                            return (
                                <option value={item}>{item}</option>
                            )
                        })}
                    </select>
                </FormGroup>
                <FormGroup label={"theme".L()}>
                    <select className="form-control" {...theme}>
                        {themes.map(item => {
                            return (
                                <option value={item.key}>{item.name}</option>
                            )
                        })}
                    </select>
                </FormGroup>
                <FormGroup>
                    <div className="checkbox">
                        <label>
                            <input type="checkbox" {...onlyShowSummary}/>
                            {"showOnlyArticleSummary".L()}
                        </label>
                    </div>
                </FormGroup>
                <FormGroup>
                    <div className="checkbox">
                        <label>
                            <input type="checkbox" {...registration} />
                            {"registration".L()}
                        </label>
                    </div>
                </FormGroup>
                <FormGroup>
                    <button type="submit" className="btn btn-primary">
                        {"save".L()}
                    </button>
                </FormGroup>
            </form>

        )
    }
}

BasicConfigForm = reduxForm({
    form: "basicConfigForm",
    fields: ["host", "title", "description", "language", "topicsPerPage", "onlyShowSummary", "registration"],
    validate
})(BasicConfigForm)

class BasicConfig extends React.Component{
    constructor() {
        super()

        this.state = {
            config: {
                title: "",
                description: "",
                language: "en-GB",
                topicsPerPage: 10,
                theme: "default"
            },
            themes: [],
            loading: true
        }
    }

    onSubmit(model) {
        if(this.state.loading){
            return false;
        }

        this.setState({
            loading: true
        }, ()=>{
            Api.saveBasicConfig(model, response=>{
                this.setState({
                    loading: false
                });

                if(response.success){
                    Dialog.success("operationSuccessful".L());
                    window.location.reload();
                }
                else{
                    Dialog.error(response.errorMessage);
                }
            })
        });
    }

    componentDidMount() {
        this.loadData();
    }

    queryBasicConfig(callback) {
        Api.getBasicConfig(response => {
            if (response.success) {
                callback(null, response.data);
            }
            else {
                callback(response.errorMessage);
            }
        })
    }

    queryThemes(callback) {
        Api.getThemes(response => {
            if (response.success) {
                callback(null, response.data);
            }
            else {
                callback(response.errorMessage);
            }
        })
    }

    loadData() {
        Parallel([this.queryBasicConfig, this.queryThemes], (err, args) => {
            if (err) {
                this.setState({
                    loading: false
                })
                Dialog.error(err);
            }
            else {
                let basicConfig = args[0];
                let themeList = args[1];

                this.setState({
                    config: basicConfig,
                    themes: themeList,
                    loading: false
                })
            }
        })
    }

    render() {
        return (                
            <div className="content">
                <Spinner loading={this.state.loading}/>
                <div className="form-content">
                    <BasicConfigForm onSubmit={this.onSubmit.bind(this)} initialValues={this.state.config} themes={this.state.themes}></BasicConfigForm>
                </div>
            </div>
        )
    }
}

module.exports = BasicConfig