/**
 * Shell completion script generation for bash and zsh.
 *
 * Usage:
 *   hickory completion bash   # Output bash completion script
 *   hickory completion zsh    # Output zsh completion script
 *
 * Installation:
 *   # Bash (add to ~/.bashrc)
 *   eval "$(hickory completion bash)"
 *
 *   # Zsh (add to ~/.zshrc)
 *   eval "$(hickory completion zsh)"
 *
 *   # Alternatively, for zsh you can save to an fpath directory:
 *   hickory completion zsh > ~/.zsh/completions/_hickory
 *   # Then ensure ~/.zsh/completions is in your fpath before compinit
 */

const BASH_COMPLETION_SCRIPT = `#!/usr/bin/env bash

_hickory_completions() {
  local cur prev words cword
  _init_completion || return

  # Top-level commands
  local commands="login logout whoami ticket agent completion"

  # Global options
  local global_options="-h --help -V --version"

  # Subcommands
  local ticket_commands="create view list"
  local agent_commands="queue assign close"
  local completion_commands="bash zsh"

  # Options
  local login_options="-e --email -p --password -h --help"
  local agent_queue_options="-f --filter -h --help"
  local agent_queue_filter_values="all unassigned mine"

  case "\${cword}" in
    1)
      COMPREPLY=( $(compgen -W "\${commands} \${global_options}" -- "\${cur}") )
      return
      ;;
    2)
      case "\${prev}" in
        ticket)
          COMPREPLY=( $(compgen -W "\${ticket_commands}" -- "\${cur}") )
          return
          ;;
        agent)
          COMPREPLY=( $(compgen -W "\${agent_commands}" -- "\${cur}") )
          return
          ;;
        completion)
          COMPREPLY=( $(compgen -W "\${completion_commands}" -- "\${cur}") )
          return
          ;;
        login)
          COMPREPLY=( $(compgen -W "\${login_options}" -- "\${cur}") )
          return
          ;;
      esac
      ;;
    3)
      case "\${words[1]}" in
        agent)
          case "\${prev}" in
            queue)
              COMPREPLY=( $(compgen -W "\${agent_queue_options}" -- "\${cur}") )
              return
              ;;
          esac
          ;;
        ticket)
          case "\${prev}" in
            view)
              # No static completions for ticket number
              return
              ;;
          esac
          ;;
      esac
      ;;
    4)
      case "\${words[1]}" in
        agent)
          case "\${words[2]}" in
            queue)
              case "\${prev}" in
                -f|--filter)
                  COMPREPLY=( $(compgen -W "\${agent_queue_filter_values}" -- "\${cur}") )
                  return
                  ;;
              esac
              ;;
          esac
          ;;
      esac
      ;;
  esac
}

complete -F _hickory_completions hickory
`;

const ZSH_COMPLETION_SCRIPT = `# Zsh completion for hickory
# Install: eval "$(hickory completion zsh)"

_hickory() {
  local -a commands ticket_commands agent_commands completion_commands
  local -a agent_queue_filter_values

  commands=(
    'login:Authenticate with the Hickory API'
    'logout:Clear authentication credentials'
    'whoami:Show current authenticated user'
    'ticket:Manage support tickets'
    'agent:Agent commands for managing support tickets'
    'completion:Generate shell completion scripts'
  )

  ticket_commands=(
    'create:Create a new support ticket interactively'
    'view:View details of a specific ticket'
    'list:List all your tickets'
  )

  agent_commands=(
    'queue:View the agent ticket queue'
    'assign:Assign a ticket to yourself'
    'close:Close a ticket with resolution notes'
  )

  completion_commands=(
    'bash:Generate bash completion script'
    'zsh:Generate zsh completion script'
  )

  agent_queue_filter_values=(
    'all:Show all tickets'
    'unassigned:Show only unassigned tickets'
    'mine:Show only tickets assigned to me'
  )

  _arguments -C \\
    '(- *)'{-h,--help}'[Show help]' \\
    '(- *)'{-V,--version}'[Show version]' \\
    '1:command:->command' \\
    '*::arg:->args'

  case "\$state" in
    command)
      _describe -t commands 'hickory command' commands
      ;;
    args)
      case "\$words[1]" in
        ticket)
          _arguments -C \\
            '1:subcommand:->subcmd' \\
            '*::arg:->subargs'
          case "\$state" in
            subcmd)
              _describe -t ticket_commands 'ticket command' ticket_commands
              ;;
            subargs)
              case "\$words[1]" in
                view)
                  _arguments '1:ticket:' ;;
              esac
              ;;
          esac
          ;;
        agent)
          _arguments -C \\
            '1:subcommand:->subcmd' \\
            '*::arg:->subargs'
          case "\$state" in
            subcmd)
              _describe -t agent_commands 'agent command' agent_commands
              ;;
            subargs)
              case "\$words[1]" in
                queue)
                  _arguments \\
                    '(-f --filter)'{-f,--filter}'[Filter tickets]:filter:->filter'
                  case "\$state" in
                    filter)
                      _describe -t filter 'filter type' agent_queue_filter_values
                      ;;
                  esac
                  ;;
                assign)
                  _arguments '1:ticket:' ;;
                close)
                  _arguments '1:ticket:' ;;
              esac
              ;;
          esac
          ;;
        completion)
          _arguments '1:shell:(bash zsh)'
          ;;
        login)
          _arguments \\
            '(-e --email)'{-e,--email}'[User email]:email:' \\
            '(-p --password)'{-p,--password}'[User password]:password:'
          ;;
      esac
      ;;
  esac
}

compdef _hickory hickory
`;

export function generateCompletion(shell: string): void {
  switch (shell) {
    case 'bash':
      process.stdout.write(BASH_COMPLETION_SCRIPT);
      break;
    case 'zsh':
      process.stdout.write(ZSH_COMPLETION_SCRIPT);
      break;
    default:
      console.error(`Unsupported shell: ${shell}`);
      console.error('Supported shells: bash, zsh');
      console.error('');
      console.error('Usage:');
      console.error('  hickory completion bash');
      console.error('  hickory completion zsh');
      process.exit(1);
  }
}
