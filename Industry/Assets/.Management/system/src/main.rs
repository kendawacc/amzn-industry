use std::collections::HashMap;
use std::path::Path;
use std::time::Duration;

use tokio::fs;
use tokio::fs::File;
use tokio::io::AsyncWriteExt;
use tokio::process::Command;
use tokio::time::sleep;

use anyhow::Result;
use rust_embed::RustEmbed;

#[derive(RustEmbed)]
#[folder = "module/"]
struct Module;

#[tokio::main]
async fn main() -> Result<()>
{
    inittask().await?;
    Ok(())
}

pub async fn packagetask() -> Result<()>
{
    if let Some(module)
        = Module::get("package")
    {
        let content
            = std::str::from_utf8(&module.data)?;

        for val in content.lines()
        {
            let pkg
            = val.trim();

            if pkg
                .is_empty()
            { continue; }

            let cmd = Command::new("pacman")
                .arg("-Syu")
                .arg(pkg)
                .arg("--noconfirm")
                .output()
                .await?;

            let boolean
            = cmd.status.success()
                == false;

            if boolean
            {
                if cmd.stdout.is_empty()
                    == false
                {
                    eprintln!("Package STDOUT Error: \n{}",
                              String::from_utf8_lossy(&cmd.stdout));
                }
                if cmd.stderr.is_empty()
                    == false
                {
                    eprintln!("Package STDERR Error: \n{}",
                              String::from_utf8_lossy(&cmd.stderr));
                }
                std::process::exit(0);
            }
        }
    }
    Ok(())
}

pub async fn configuretask() -> Result<()>
{
    let mut map: HashMap<&str, &str>
        = HashMap::new();

    {
        // bluetooth
        {
            map.insert(
                "/etc/bluetooth/main.conf",
                "runtime/etc.bluetooth.main");
        }

        // grub
        {
            {
                map.insert(
                    "/etc/default/grub",
                    "runtime/etc.default.grub");
            }

            {
                map.insert(
                    "/etc/grub.d/10_linux",
                    "runtime/etc.grub.d.10_linux");
            }
        }

        // pulse
        {
            {
                map.insert(
                    "/etc/pulse/client.conf",
                    "runtime/etc.pulse.client");
            }

            {
                map.insert(
                    "/etc/pulse/default.pa",
                    "runtime/etc.pulse.default");
            }
        }

        // systemd
        {
            {
                map.insert(
                    "/etc/systemd/system/getty@tty1.service.d/noshow.conf",
                    "runtime/etc.systemd.noshow");
            }

            {
                map.insert(
                    "/etc/systemd/system/getty@tty1.service.d/skip-prompt.conf",
                    "runtime/etc.systemd.skip-prompt");
            }

            {
                map.insert(
                    "/etc/systemd/system/system.service",
                    "runtime/etc.systemd.system");
            }
        }

        // openbox
        {
            map.insert(
                "/root/.config/openbox/rc.xml",
                "runtime/root.openbox.rc");
        }
    }

    for (key, value) in map
    {
        let Some(module) = Module::get(value) else
        { continue; };

        if let Some(parent) = Path::new(key).parent()
        { fs::create_dir_all(parent).await?; }

        let mut file = File::create(key).await?;
        file.write_all(&module.data).await?;
    }

    Ok(())
}

pub async fn runtimetask() -> Result<()>
{
    let arr
        = [
        "systemctl enable NetworkManager",
        "systemctl enable wpa_supplicant",
        "systemctl enable bluetooth",
        "systemctl enable getty@ttyN",
        "systemctl enable getty@tty1",
        "systemctl enable system",
        "systemctl daemon-reload",

        "chmod 0700 -R /indus/",
        "grub-mkconfig -o /boot/grub/grub.cfg"
    ];

    for val in arr.iter()
    {
        let cmd = Command::new("sh")
            .arg("-c")
            .arg(val)
            .output()
            .await?;

        let boolean
        = cmd.status.success()
        == false;

        if boolean
        {
            if cmd.stdout.is_empty()
                == false
            {
                eprintln!("Runtime STDOUT Error: \n{}",
                          String::from_utf8_lossy(&cmd.stdout));
            }
            if cmd.stderr.is_empty()
                == false
            {
                eprintln!("Runtime STDERR Error: \n{}",
                          String::from_utf8_lossy(&cmd.stderr));
            }
            std::process::exit(0);
        }
    }

    Ok(())
}

pub async fn systemtask() -> Result<()>
{
    {
        {
            let arr
                = [
                "system/video",
                "system/audio",
            ];

            for val in arr.iter()
            {
                if let Some(module)
                    = Module::get(val)
                {
                    let content
                        = std::str::from_utf8(&module.data)?;

                    let cmd = Command::new("sh")
                        .arg("-c")
                        .arg(content)
                        .spawn();

                    if cmd.is_err()
                    {
                        println!("System Error: \n{}", val);
                        std::process::exit(0);
                    }
                }

                sleep(Duration::from_millis(5)).await;
            }
        }

        {
            let mut index : i32
                = 0;

            loop
            {
                if let Some(module)
                    = Module::get("system/set")
                {
                    let content
                        = std::str::from_utf8(&module.data)?;

                    let cmd = Command::new("sh")
                        .arg("-c")
                        .arg(content)
                        .output()
                        .await?;

                    let booolean
                        = cmd.status.success();

                    if booolean
                    { break; }

                    else if
                    index == 1400
                    {
                        if cmd.stdout.is_empty()
                            == false
                        {
                            eprintln!("System STDOUT Error: \n{}",
                                      String::from_utf8_lossy(&cmd.stdout));
                        }
                        if cmd.stderr.is_empty()
                            == false
                        {
                            eprintln!("System STDERR Error: \n{}",
                                      String::from_utf8_lossy(&cmd.stderr));
                        }

                        std::process::exit(0);
                    }
                }

                index += 1;
                sleep(Duration::from_millis(5)).await;
            }
        }

        {
            if let Some(module)
                = Module::get("system/window")
            {
                let content
                    = std::str::from_utf8(&module.data)?;

                let cmd = Command::new("sh")
                    .arg("-c")
                    .arg(content)
                    .spawn();

                if cmd.is_err()
                {
                    println!("System Error: \n{}", "window");
                    std::process::exit(0);
                }
            }
        }
    }

    {
        let path
            = Path::new("/indus/flag/firmware");

        if path.exists()
        {
            if let Some(module)
                = Module::get("system/firmware")
            {
                let content
                    = std::str::from_utf8(&module.data)?;

                let cmd = Command::new("sh")
                    .arg("-c")
                    .arg(content)
                    .output()
                    .await?;

                let boolean
                    = cmd.status.success()
                    == false;

                if boolean
                {
                    if cmd.stdout.is_empty()
                        == false
                    {
                        eprintln!("Firmware STDOUT Error: \n{}",
                                  String::from_utf8_lossy(&cmd.stdout));
                    }
                    if cmd.stderr.is_empty()
                        == false
                    {
                        eprintln!("Firmware STDERR Error: \n{}",
                                  String::from_utf8_lossy(&cmd.stderr));
                    }
                }
            }
        }
        else
        {
            if let Some(module)
                = Module::get("system/oses")
            {
                let content
                    = std::str::from_utf8(&module.data)?;

                let cmd = Command::new("sh")
                    .arg("-c")
                    .arg(content)
                    .output()
                    .await?;

                let boolean
                    = cmd.status.success()
                    == false;

                if boolean
                {
                    if cmd.stdout.is_empty()
                        == false
                    {
                        eprintln!("System STDOUT Error: \n{}",
                                  String::from_utf8_lossy(&cmd.stdout));
                    }
                    if cmd.stderr.is_empty()
                        == false
                    {
                        eprintln!("System STDERR Error: \n{}",
                                  String::from_utf8_lossy(&cmd.stderr));
                    }
                }
            }
        }
    }
    Ok(())
}

pub async fn inittask() -> Result<()>
{
    let path
        = Path::new("/indus/flag/init");

    if path.exists()
        == false
    {
        packagetask().await?;
        configuretask().await?;
        runtimetask().await?;
        File::create(path).await?;
    }
    else { systemtask().await?; }
    std::process::exit(0);
    Ok(())
}