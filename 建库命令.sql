create database shuyisen_kcsj;
use shuyisen_kcsj;

-- 受教育程度表
create table `shuyisen_edu` (
    `eCode` int auto_increment not null primary key,
    `eDescription` varchar(6) not null unique
);

-- 职务表
create table `shuyisen_job` ( 
    `jCode` int auto_increment primary key, 
    `jDescription` varchar(10) not null unique
);

-- 职务变动类型表
create table `shuyisen_change` ( 
    `cCode` int auto_increment primary key,
    `cDescription` varchar(20) not null unique
);

-- 员工状态
create table `shuyisen_staff_state` (
    `stateCode` int auto_increment primary key,
    `stateDescription` varchar(10) not null unique
);

-- 部门表
CREATE TABLE `shuyisen_department` ( 
    `dID` int auto_increment primary key,
    `dName` varchar(16) not null,
    `dIntro` varchar(20) default null,
    `managerID` varchar(15) default null
);

-- 员工表
create table `shuyisen_staff` ( 
    `sID` varchar(15) not null primary key,
    `sPassword` varchar(20) not null, 
    `sName` varchar(20) not null, 
    `sSex` varchar(2) not null, 
    `sBirthday` date not null, 
    `dID` int not null, 
    `jCode` int not null, 
    `eCode` int not null,
    `sSpecialty` varchar(20) not null, 
    `sAddress` varchar(40) not null, 
    `sTel` varchar(15) not null, 
    `sEmail` varchar(50) not null, 
    `stateCode` int not null, 
    `sRemark` varchar(20) default null,
    `sAuthority` varchar(20) not null,
    foreign key(`dID`) references shuyisen_department(`dID`),
    foreign key(`jCode`) references shuyisen_job(`jCode`),
    foreign key(`eCode`) references shuyisen_edu(`eCode`),
    foreign key(`stateCode`) references shuyisen_staff_state(`stateCode`),
    index(`sName`),
    index(`dID`, `jCode`),
    index(`stateCode`)
);

-- 添加部门表的外键关系
alter table `shuyisen_department`
add constraint `department_fk_1` foreign key(`managerID`) references `shuyisen_staff`(`sID`) on delete set null on update cascade;

-- 人事变动表
CREATE TABLE `shuyisen_personnel` ( 
    `pID` int auto_increment primary key,
    `sID` varchar(15) not null,
    `cCode` int not null, 
    `pDescription` varchar(100) not null,
    constraint `personnel_fk_1` foreign key (`sID`) references `shuyisen_staff` (`sID`) on delete cascade on update cascade,
    constraint `personnel_fk_2` foreign key (`cCode`) references `shuyisen_change` (`cCode`),
    index(`sID`)
);

-- 初始化基本表

-- 受教育程度表
insert into `shuyisen_edu` (eDescription) values('小学');
insert into `shuyisen_edu` (eDescription) values('初中');

insert into `shuyisen_edu` (eDescription) values('职高');

insert into `shuyisen_edu` (eDescription) values('高中');

insert into `shuyisen_edu` (eDescription) values('中专');

insert into `shuyisen_edu` (eDescription) values('大专');

insert into `shuyisen_edu` (eDescription) values('本科');

insert into `shuyisen_edu` (eDescription) values('硕士');

insert into `shuyisen_edu` (eDescription) values('博士');

insert into `shuyisen_edu` (eDescription) values('博士后');

-- 职务表
insert into `shuyisen_job` (jDescription) values('普通员工');
insert into `shuyisen_job` (jDescription) values('副组长');
insert into `shuyisen_job` (jDescription) values('组长');
insert into `shuyisen_job` (jDescription) values('副主任');
insert into `shuyisen_job` (jDescription) values('主任');
insert into `shuyisen_job` (jDescription) values('副经理');
insert into `shuyisen_job` (jDescription) values('经理');

-- 员工状态表
insert into `shuyisen_staff_state` (stateDescription) values('在职');
insert into `shuyisen_staff_state` (stateDescription) values('离职');
insert into `shuyisen_staff_state` (stateDescription) values('休假');

-- 职务变动类型表
insert into `shuyisen_change` (cDescription) values('入职');
insert into `shuyisen_change` (cDescription) values('职务变动');
insert into `shuyisen_change` (cDescription) values('调岗');
insert into `shuyisen_change` (cDescription) values('辞退');

-- 部门表
insert into `shuyisen_department` (dName,dIntro) values('行政部','负责处理公司的日常行政工作');
insert into `shuyisen_department` (dName,dIntro) values('财务部','负责处理公司的财务事务');
insert into `shuyisen_department` (dName,dIntro) values('人事部','负责公司的人力资源管理');
insert into `shuyisen_department` (dName,dIntro) values('技术部','负责管理和维护公司的技术基础设施');
insert into `shuyisen_department` (dName,dIntro) values('生产部','负责管理产品生产');

-- 新员工入职触发器
delimiter //
create trigger trg_new_staff
after insert on shuyisen_staff
for each row
begin
    insert into shuyisen_personnel (sID,cCode,pDescription)
    values(
        new.sID,
        (select cCode from shuyisen_change where cDescription = '入职'),
        '新员工入职'
    );
end //
delimiter ;

-- 员工职务变动触发器
delimiter //
create trigger trg_staff_change
after update on shuyisen_staff
for each row
begin
    if(new.jCode != old.jCode) then
        insert into shuyisen_personnel (sID,cCode,pDescription)
        values(
            new.sID,
            (select cCode from shuyisen_change where cDescription = '职务变动'),
            concat('从', (select jDescription from shuyisen_job where jCode = old.jCode), '变更为', (select jDescription from shuyisen_job where jCode = new.jCode))
        );
    end if;
    if(new.jCode = (select jCode from shuyisen_job where jDescription = '经理')) then
        update shuyisen_department set managerID = new.sID where dID = new.dID;
    end if;
end //
delimiter ;

-- 部门经理变更触发器
delimiter //
create trigger trg_department_manager_change
after update on shuyisen_department
for each row
begin
    if(new.managerID != old.managerID) then
        insert into shuyisen_personnel (sID,cCode,pDescription)
        values (
            new.managerID,
            (select cCode from shuyisen_change where cDescription = '职务变动'),
            concat('被任命为',new.dName, '的经理')
        );

        if old.managerID is not null then
            insert into shuyisen_personnel (sID,cCode,pDescription)
            values(
                old.managerID,
                (select cCode from shuyisen_change where cDescription = '职务变动'),
                concat('不再担任', new.dName, '的经理')
            );
        end if;
    end if;
end //
delimiter ;

-- 员工离职处理存储过程
delimiter //
create procedure proc_staff_leave(in p_sid varchar(15),in p_reason varchar(100))
begin
    declare v_job_name varchar(10);
    declare v_dept_name varchar(16);
    declare v_is_manager int;

    select jDescription into v_job_name from shuyisen_job where jCode = (select jCode from shuyisen_staff where sID = p_sid);
    select dName into v_dept_name from shuyisen_department where dID = (select dID from shuyisen_staff where sID = p_sid);

    select count(*) into v_is_manager from shuyisen_department where managerID = p_sid;

    update shuyisen_staff set stateCode = 1 where sID = p_sid;

    if v_is_manager > 0 then
        update shuyisen_department set managerID = null where managerID = p_sid;
    end if;

    insert into shuyisen_personnel (sID,cCode,pDescription)
    values (
        p_sid,
        (select cCode from shuyisen_change where cDescription = '离职'),
        concat('离职。原职务：',v_job_name,',原部门：',v_dept_name,'。原因：',p_reason)
    );
end //
delimiter;



-- 员工调动存储过程
delimiter //
create procedure proc_staff_transfer(in p_sID varchar(15),in p_newDeptID int,in p_newJobID int,in p_reason varchar(100))
begin
    declare old_dept_name varchar(16);
    declare new_dept_name varchar(16);
    declare old_job_name varchar(10);
    declare new_job_name varchar(10);

    select dName into old_dept_name from shuyisen_department where dID = (select dID from shuyisen_staff where sID = p_sID);
    select dName into new_dept_name from shuyisen_department where dID = p_newDeptID;
    
    select jDescription into old_job_name from shuyisen_job where jCode = (select jCode from shuyisen_staff where sID = p_sID);
    select jDescription into new_job_name from shuyisen_job where jCode = p_newJobID;

    update shuyisen_staff set dID = p_newDeptID, jCode = p_newJobID where sID = p_sID;

    insert into shuyisen_personnel (sID,cCode,pDescription)
    values (
        p_sID,
        (select cCode from shuyisen_change where cDescription = '调岗'),
        concat('从',old_dept_name,'的',old_job_name,'调动到',new_dept_name,'的',new_job_name,'。原因：',p_reason)
    );

    if p_newJobID = (select jCode from shuyisen_job where jDescription = '经理') then
        update shuyisen_department set managerID = p_sID where dID = p_newDeptID;
    end if;
end //
delimiter ;

-- 员工休假存储过程
delimiter //
create procedure proc_staff_vacation(int p_sID varchar(15),in p_reason varchar(100))
begin
    update shuyisen_staff set stateCode = (select stateCode from shuyisen_staff_state where stateDescription = '休假');
    
    insert into shuyisen_personnel (sID,cCode,pDescription)
    values (
        p_sID,
        (select cCode from shuyisen_change where cDescription = '休假'),
        concat("休假。原因：",p_reason)
    );
end //
delimiter;

-- 员工复工存储过程
delimiter //
create procedure proc_staff_rework(in p_sID varchar(15),in p_reason varchar(100))
begin
    update shuyisen_staff set stateCode = (select stateCode from shuyisen_staff_state where stateDescription = '在职');
    
    insert into shuyisen_personnel (sID,cCode,pDescription)
    values (
        p_sID,
        (select cCode from shuyisen_change where cDescription = '返岗'),
        concat("返岗。原因：",p_reason)
    );
end //
delimiter;

-- 录入新员工信息存储过程
delimiter //
create procedure proc_add_staff(in p_sID varchar(15),in p_sPassword varchar(20),in p_sName varchar(20),in p_sSex varchar(2),in p_sBirthday date,in p_dName varchar(16),
                                in p_jDescription varchar(10),in p_eDescription varchar(6),in p_sSpecialty varchar(20),in p_sAddress varchar(40),in p_sTel varchar(15),
                                in p_sEmail varchar(50),in p_sAutority varchar(20))
begin
    declare p_dID int;
    declare p_jCode int;
    declare p_eCode int;
    declare p_stateCode int;

    select dID into p_dID from shuyisen_department where dName = p_dName;
    select jCode into p_jCode from shuyisen_job where jDescription = p_jDescription;
    select eCode into p_eCode from shuyisen_edu where eDescription = p_eDescription;
    select stateCode into p_stateCode from shuyisen_staff_state where stateDescription = '在职';

    insert into shuyisen_staff values(p_sID,p_sPassword,p_sName,p_sSex,p_sBirthday,p_dID,p_jCode,p_eCode,p_sSpecialty,p_sAddress,p_sTel,p_sEmail,p_stateCode,'',p_sAutority);
end //

-- 修改员工信息存储过程
delimiter //
create procedure proc_edit_staff(in p_sID varchar(15),in p_sName varchar(20),in p_sSex varchar(2),in p_sBirthday date,in p_eDescription varchar(6),
                                 in p_sSpecialty varchar(20),in p_sAddress varchar(40),in p_sTel varchar(15),in p_sEmail varchar(50))
begin
    declare p_eCode int;

    select eCode into p_eCode from shuyisen_edu where eDescription = p_eDescription;

    update shuyisen_staff set sName = p_sName,sSex = p_sSex,sBirthday = p_sBirthday,eCode = p_eCode,sSpecialty = p_sSpecialty,sAddress = p_sAddress,sTel = p_sTel,sEmail = p_sEmail where sID = p_sID;
end //

-- 插入员工数据
INSERT INTO `shuyisen_staff` (`sID`, `sPassword`, `sName`, `sSex`, `sBirthday`, `dID`, `jCode`, `eCode`, `sSpecialty`, `sAddress`, `sTel`, `sEmail`, `stateCode`, `sRemark`, `sAuthority`) VALUES
('100001', '123456', '张三', '女', '1988-06-20', 3, 7, 7, '人力资源管理', '北京市海淀区中关村', '13800138006', 'hrzhang@company.com', 1, '人事经理', '管理员'),
('100002', '123456', '李四', '男', '1992-03-15', 3, 3, 7, '劳动关系', '北京市朝阳区国贸', '13900139007', 'hrli@company.com', 1, '人事专员', '管理员'),
('100003', '123456', '王五', '男', '1990-05-15', 4, 3, 7, '软件开发', '北京市海淀区西二旗', '13800138001', 'wangjs@company.com', 1, '技术组长', '员工'),
('100004', '123456', '赵六', '女', '1985-08-22', 2, 6, 8, '财务管理', '上海市浦东新区', '13900139002', 'zhaocw@company.com', 1, '财务副经理', '员工'),
('100005', '123456', '钱七', '男', '1993-11-30', 5, 2, 6, '机械制造', '广州市黄埔区', '13500135003', 'qianprod@company.com', 1, '生产副组长', '员工');


-- 所有员工信息视图
create view view_all_staff as select s.sID,s.sPassword,s.sName,s.sSex,s.sBirthday,d.dName,j.jDescription,e.eDescription,s.sSpecialty,s.sAddress,s.sTel,s.sEmail,state.stateDescription from shuyisen_staff s join shuyisen_department d on s.dID = d.dID join shuyisen_job j on s.jCode = j.jCode join shuyisen_edu e on s.eCode = e.eCode join shuyisen_staff_state state on s.stateCode = state.stateCode;

